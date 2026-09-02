using System.Text;
using System.Text.Json;
using Dapper;
using FinanceTracker.Data;
using FinanceTracker.Models.Statement;

namespace FinanceTracker.Services
{
    public interface IStatementService
    {
        Task<StatementExtractResponse> ExtractStatement(Guid userPublicId, IFormFile file);
        Task<List<ImportTransaction>> CheckDuplicates(Guid userPublicId, DuplicateCheckRequest request);
        Task<StatementImportResult> ImportStatement(Guid userPublicId, StatementImportRequest request);
    }

    public class StatementService : IStatementService
    {
        private readonly DbContext _db;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions CamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public StatementService(DbContext db, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        private async Task<int> GetUserId(Guid userPublicId)
        {
            using var connection = _db.CreateConnection();
            var p = new DynamicParameters();
            p.Add("@PublicId", userPublicId);
            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserByPublicId", p,
                commandType: System.Data.CommandType.StoredProcedure);
            if (user == null) throw new Exception("User not found.");
            return (int)user.Id;
        }

        public async Task<StatementExtractResponse> ExtractStatement(Guid userPublicId, IFormFile file)
        {
            var userId = await GetUserId(userPublicId);

            using var connection = _db.CreateConnection();
            var expenseTypes = await connection.QueryAsync<string>(
                @"SELECT ExpenseName FROM ExpenseTypes
                  WHERE UserId = @UserId AND IsActive = 1 AND IsSystemManaged = 0
                  ORDER BY ExpenseName",
                new { UserId = userId });

            var categoryList = string.Join(", ", expenseTypes);
            var apiKey = _configuration["Anthropic:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Anthropic API key not configured.");

            var systemPrompt = $@"You are a financial statement parser. Extract all transactions from the provided bank or credit card statement.

Return ONLY valid JSON. No markdown, no explanation, no code blocks, no backticks.

Required JSON structure:
{{
  ""statementType"": ""bank"" or ""creditcard"",
  ""transactions"": [
    {{
      ""date"": ""YYYY-MM-DD"",
      ""description"": ""original transaction description from statement"",
      ""amount"": 123.45,
      ""type"": ""debit"" or ""credit"",
      ""suggestedCategory"": ""category name or null""
    }}
  ]
}}

Rules:
- statementType: use ""bank"" for chequing/savings statements, ""creditcard"" for credit card statements
- date: always YYYY-MM-DD format
- amount: always a positive decimal number, never negative
- type:
    Bank statement  → ""credit"" = money coming IN (deposits, salary, GIC, transfers in, PAY entries)
                      ""debit""  = money going OUT (purchases, withdrawals, transfers out, bill payments)
    Credit card     → ""debit""  = purchases charged to card
                      ""credit"" = payments made to the card
- PAY or Wage/salary entries are ALWAYS ""credit"" regardless of company name
- suggestedCategory: match description to one of these expense types if applicable:
    {categoryList}
    Common mappings: Tim Hortons / McDonald's / restaurants = Dine out
                     Walmart / FreshCo / Shoppers = Groceries
                     Netflix / Spotify / Amazon Prime = Subscriptions
                     Fido / Rogers bill pay = Internet or Phone
    For salary/payroll/PAY/deposits/transfers: use null
    If no match: use null
- SKIP these rows entirely: opening balance, closing balance, running balance, balance forward, summary totals, section headers
- Include ALL actual transaction rows";

            var extension = Path.GetExtension(file.FileName).ToLower();
            object messageContent;

            if (extension == ".csv" || extension == ".txt")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var textContent = await reader.ReadToEndAsync();
                messageContent = new[]
                {
                    new { type = "text", text = $"Here is the statement content:\n\n{textContent}\n\nExtract all transactions and return JSON only." }
                };
            }
            else
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                var mediaType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".png" => "image/png",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    _ => "application/pdf"
                };
                messageContent = new object[]
                {
                    new { type = "document", source = new { type = "base64", media_type = mediaType, data = base64 } },
                    new { type = "text", text = "Extract all transactions from this statement and return JSON only." }
                };
            }

            var requestBody = new
            {
                model = "claude-haiku-4-5-20251001",
                max_tokens = 4096,
                system = systemPrompt,
                messages = new[] { new { role = "user", content = messageContent } }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            httpRequest.Headers.Add("x-api-key", apiKey);
            httpRequest.Headers.Add("anthropic-version", "2023-06-01");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.SendAsync(httpRequest);
            var responseJson = await httpResponse.Content.ReadAsStringAsync();
            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception($"Claude API error: {responseJson}");

            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            content = content.Trim();
            if (content.StartsWith("```"))
            {
                var firstNewline = content.IndexOf('\n');
                content = firstNewline >= 0 ? content[(firstNewline + 1)..] : content[3..];
                var lastFence = content.LastIndexOf("```");
                if (lastFence >= 0) content = content[..lastFence].Trim();
            }

            var result = JsonSerializer.Deserialize<StatementExtractResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new StatementExtractResponse();
        }

        public async Task<List<ImportTransaction>> CheckDuplicates(Guid userPublicId, DuplicateCheckRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);

            var p = new DynamicParameters();
            p.Add("@JsonData", JsonSerializer.Serialize(new
            {
                UserId = userId,
                Transactions = request.Transactions
            }, CamelCase));

            var duplicates = await connection.QueryAsync<ImportTransaction>(
                "sp_CheckDuplicateTransactions", p,
                commandType: System.Data.CommandType.StoredProcedure);

            return duplicates.ToList();
        }

        public async Task<StatementImportResult> ImportStatement(Guid userPublicId, StatementImportRequest request)
        {
            using var connection = _db.CreateConnection();
            var userId = await GetUserId(userPublicId);
            var result = new StatementImportResult();

            if (request.StatementType == "bank")
            {
                // Resolve bank account
                var accountId = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT Id FROM UserAccounts WHERE PublicId = @PublicId AND UserId = @UserId",
                    new { PublicId = request.AccountPublicId, UserId = userId });
                if (accountId == 0)
                    throw new Exception("Account not found.");

                // Resolve debit payment method
                var debitMethodId = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT Id FROM PaymentMethods WHERE MethodName = 'E-Transfer'");

                // ─── Debits → Transactions table ─────────────────────
                var debits = request.Transactions
                    .Where(t => t.Type == "debit" && t.ExpenseTypeId.HasValue)
                    .ToList();

                if (debits.Any())
                {
                    var p = new DynamicParameters();
                    p.Add("@JsonData", JsonSerializer.Serialize(new
                    {
                        UserId = userId,
                        AccountId = accountId,
                        PaymentMethodId = debitMethodId,
                        Transactions = debits
                    }, CamelCase));
                    await connection.ExecuteAsync(
                        "sp_BulkInsertTransactions", p,
                        commandType: System.Data.CommandType.StoredProcedure);
                    result.InsertedTransactions += debits.Count;
                }

                // ─── Credits — split by type ──────────────────────────
                var credits = request.Transactions
                    .Where(t => t.Type == "credit" && t.CreditType != "skip")
                    .ToList();

                var depositCredits = credits
                    .Where(t => t.CreditType == "deposit" || string.IsNullOrEmpty(t.CreditType))
                    .ToList();

                var salaryCredits = credits
                    .Where(t => t.CreditType != null && t.CreditType.StartsWith("salary"))
                    .ToList();

                // General deposits → MonthlyAccountSummary
                if (depositCredits.Any())
                {
                    var p = new DynamicParameters();
                    p.Add("@JsonData", JsonSerializer.Serialize(new
                    {
                        UserId = userId,
                        AccountId = accountId,
                        Transactions = depositCredits
                    }, CamelCase));
                    await connection.ExecuteAsync(
                        "sp_BulkUpsertDeposits", p,
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                // Salary credits → MonthlySalary per employment
                foreach (var salaryRow in salaryCredits)
                {
                    if (salaryRow.EmploymentId == null) continue;
                    var date = DateTime.Parse(salaryRow.Date);
                    var sp = new DynamicParameters();
                    sp.Add("@JsonData", JsonSerializer.Serialize(new
                    {
                        UserId = userId,
                        EmploymentId = salaryRow.EmploymentId,
                        Month = date.Month,
                        Year = date.Year,
                        NetPay = salaryRow.Amount
                    }));
                    await connection.ExecuteAsync(
                        "sp_UpsertMonthlySalary", sp,
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                result.MonthsAffected = request.Transactions
                    .Select(t => new { Month = DateTime.Parse(t.Date).Month, Year = DateTime.Parse(t.Date).Year })
                    .Distinct().Count();
            }
            else if (request.StatementType == "creditcard")
            {
                // Resolve credit card
                var creditCardId = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT Id FROM CreditCards WHERE PublicId = @PublicId AND UserId = @UserId",
                    new { PublicId = request.CreditCardPublicId, UserId = userId });
                if (creditCardId == 0)
                    throw new Exception("Credit card not found.");

                // Purchases (debits) → Transactions with Credit payment method
                var purchases = request.Transactions
                    .Where(t => t.Type == "debit" && t.ExpenseTypeId.HasValue)
                    .ToList();

                if (purchases.Any())
                {
                    var p = new DynamicParameters();
                    p.Add("@JsonData", JsonSerializer.Serialize(new
                    {
                        UserId = userId,
                        CreditCardId = creditCardId,
                        Transactions = purchases
                    }, CamelCase));
                    await connection.ExecuteAsync(
                        "sp_BulkUpsertCreditCardTransactions", p,
                        commandType: System.Data.CommandType.StoredProcedure);
                    result.InsertedTransactions += purchases.Count;
                }

                result.MonthsAffected = request.Transactions
                    .Select(t => new { Month = DateTime.Parse(t.Date).Month, Year = DateTime.Parse(t.Date).Year })
                    .Distinct().Count();
            }

            // Recalculate all totals after import
            using var conn2 = _db.CreateConnection();
            var rp = new DynamicParameters();
            rp.Add("@UserId", userId);
            await conn2.ExecuteAsync(
                "sp_RecalculateAllTotals", rp,
                commandType: System.Data.CommandType.StoredProcedure);

            return result;
        }
    }
}
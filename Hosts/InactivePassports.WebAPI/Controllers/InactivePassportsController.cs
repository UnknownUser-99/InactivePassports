using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Entities.Database;
using InactivePassports.Data.Entities.WebAPI;
using Microsoft.AspNetCore.Mvc;

namespace InactivePassports.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InactivePassportsController : Controller
    {
        private readonly IWebAPI _repository;

        public InactivePassportsController(IWebAPI repository)
        {
            _repository = repository;
        }

        [HttpPost("FindPassport")]
        public async Task<IActionResult> FindPassport([FromBody] PassportRequest seriesNumber)
        {
            if (seriesNumber == null)
            {
                return BadRequest("Передан пустой объект.");
            }

            var result = await _repository.FindPassport(seriesNumber);

            return Ok(CreateFindResult(seriesNumber, result));
        }

        [HttpPost("PassportHistory")]
        public async Task<IActionResult> GetPassportHistory([FromBody] PassportRequest seriesNumber)
        {
            if (seriesNumber == null)
            {
                return BadRequest("Передан пустой объект.");
            }

            var result = await _repository.GetPassportHistory(seriesNumber);

            if (result.Length == 0)
            {
                return NotFound(new { Message = "Паспорт не найден." });
            }

            return Ok(CreatePassportHistoryResult(result, seriesNumber));
        }

        [HttpPost("DateHistory")]
        public async Task<IActionResult> GetDateHistory([FromBody] DateRequest dateRange)
        {
            if (dateRange == null)
            {
                return BadRequest("Передан пустой объект.");
            }

            var result = await _repository.GetDateHistory(dateRange.StartDate.Date, dateRange.EndDate.Date);

            if (result.Length == 0)
            {
                return NotFound(new { Message = "Операции не найдены." });
            }

            return Ok(result);
        }

        private static FindResult CreateFindResult(PassportRequest seriesNumber, bool result)
        {
            return new FindResult
            {
                Series = seriesNumber.Series,
                Number = seriesNumber.Number,
                Result = result
            };
        }

        private static PassportHistoryResult CreatePassportHistoryResult(Operation[] operations, PassportRequest seriesNumber)
        {
            DateAction[] actions = new DateAction[operations.Length];

            for (int i = 0; i < operations.Length; i++)
            {
                actions[i] = new DateAction
                {
                    Date = DateOnly.FromDateTime(operations[i].Date),
                    Action = operations[i].Action
                };
            }

            return new PassportHistoryResult
            {
                Id = operations[0].Passport,
                Series = seriesNumber.Series,
                Number = seriesNumber.Number,
                Actions = actions
            };
        }
    }
}
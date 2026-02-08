#nullable enable
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v2.DTOs;
using PracticalWork.Library.Contracts.v2.Requests;

namespace PracticalWork.Library.Controllers.Api.v2
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        /// <summary>
        /// Логирование события активности системы
        /// </summary>
        /// <param name="log">Объект активности</param>
        /// <returns>HTTP 200 при успехе</returns>
        [HttpPost("activity")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> LogActivity([FromBody] ActivityLogDto log)
        {
            if (log == null || log.Id == Guid.Empty)
                return BadRequest("EventId обязателен");

            await _reportsService.LogActivityAsync(log);
            return Ok();
        }
        
        /// <summary>
        /// Генерация CSV отчета за указанный период
        /// </summary>
        /// <param name="request">Параметры отчета</param>
        /// <returns>Метаданные сгенерированного отчета</returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(ReportMetadataDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ReportMetadataDto>> GenerateReport([FromBody] GenerateReportRequest request)
        {
            if (request == null)
                return BadRequest("Запрос не может быть пустым");

            if (request.From > request.To)
                return BadRequest("Дата начала периода не может быть больше даты окончания");

            var metadata = await _reportsService.GenerateReportAsync(request);
            return Ok(metadata);
        }

        /// <summary>
        /// Получение логов активности с фильтром и пагинацией
        /// </summary>
        [HttpGet("activity")]
        [ProducesResponseType(typeof(List<ActivityLogDto>), 200)]
        public async Task<ActionResult<List<ActivityLogDto>>> GetActivityLogs(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string[]? eventTypes,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var logs = await _reportsService.GetActivityLogsAsync(from, to, eventTypes, page, pageSize);
            return Ok(logs);
        }

        /// <summary>
        /// Получение списка отчетов (кэшируется на 24 часа)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ReportMetadataDto>), 200)]
        public async Task<ActionResult<List<ReportMetadataDto>>> GetReportsList()
        {
            var reports = await _reportsService.GetReportsListAsync();
            return Ok(reports);
        }

        /// <summary>
        /// Получение signed URL для скачивания отчета
        /// </summary>
        /// <param name="reportName">Имя отчета</param>
        [HttpGet("{reportName}/download")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<string>> DownloadReport([FromRoute] string reportName)
        {
            if (string.IsNullOrWhiteSpace(reportName))
                return BadRequest("Имя отчета не может быть пустым");

            try
            {
                var url = await _reportsService.GetReportDownloadUrlAsync(reportName);
                return Ok(url);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

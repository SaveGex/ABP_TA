using Application.DTOs;
using Application.Revenues.Queries.RevenueReport;
using Application.Revenues.Queries.RoomUtilizationReport;
using Microsoft.AspNetCore.Mvc;

namespace MainWeb.Controllers
{
    /// <summary>
    /// Provides business analytics, financial revenue metrics, and utilization statistics.
    /// </summary>
    public class ReportsController : ApiControllerBase
    {
        /// <summary>
        /// Generates a financial revenue report for a specified time frame.
        /// </summary>
        /// <param name="query">Date interval and optional room ID filter.</param>
        /// <response code="200">Revenue analytics successfully calculated.</response>
        /// <response code="400">Invalid date range selection.</response>
        [HttpGet("revenue")]
        [ProducesResponseType(typeof(RevenueReportDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RevenueReportDTO>> GetRevenue([FromQuery] GetRevenueReportQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        /// <summary>
        /// Calculates occupancy and room utilization rates for a given time period.
        /// </summary>
        /// <param name="query">Date interval and optional room ID filter.</param>
        /// <response code="200">Utilization report generated successfully.</response>
        /// <response code="400">Invalid date range parameters.</response>
        [HttpGet("utilization")]
        [ProducesResponseType(typeof(RoomUtilizationReportDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoomUtilizationReportDTO>> GetUtilization([FromQuery] GetRoomUtilizationReportQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
    }
}

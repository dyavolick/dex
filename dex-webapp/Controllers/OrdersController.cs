using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dex_webapp.Models.ViewModels;
using dex_webapp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace dex_webapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        [HttpGet("{token}/{address}")]
        public async Task<IEnumerable<OrderViewModel>> Get(string token, string address)
        {
            return await _ordersService.GetOrders(token, address);
        }

        [HttpGet("{token}")]
        public async Task<IEnumerable<OrderViewModel>> Get(string token)
        {
            return await _ordersService.GetOrders(token);
        }
    }
}
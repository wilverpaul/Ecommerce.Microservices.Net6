﻿using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
        {
            _logger = logger;
            _productService = productService;
            this._cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            List<ProductDto> products = new();
            var response = await _productService.GetAllProductsAsync<ResponseDto>(accessToken);

            if (response != null && response.IsSuccess)
            {
                products = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }

            return View(products);
        }


        public async Task<IActionResult> Details(int productId)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            ProductDto model = new();
            var response = await _productService.GetProductByIdAsync<ResponseDto>(productId, accessToken);

            if (response != null && response.IsSuccess)
            {
                model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Details")]
        [Authorize]
        public async Task<IActionResult> DetailsPost(ProductDto productDto)
        {

            CartDto cartDto = new()
            {
                CartHeader = new CartHeaderDto()
                {
                    UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value
                }
            };

            CartDetailsDto cartDetailsDto = new()
            {
                Count = productDto.Count,
                ProductId = productDto.ProductId,
            };

            var response = await _productService.GetProductByIdAsync<ResponseDto>(productDto.ProductId, "");
            if (response != null && response.IsSuccess)
            {
                cartDetailsDto.Product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }

            List<CartDetailsDto> cartDetailsDtos = new List<CartDetailsDto>();
            cartDetailsDtos.Add(cartDetailsDto);
            cartDto.CartDetails = cartDetailsDtos;

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var addToCartResponse = await _cartService.AddToCartAsync<ResponseDto>(cartDto, accessToken);

            if (addToCartResponse != null && addToCartResponse.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(productDto);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }



    }
}
﻿// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Server.Pages.App.Library
{
    using AVOne.Server.Pages.App.ECommerce.Shop;
    using AVOne.Server.Shared;

    public partial class Local : ProCompontentBase
    {
        readonly List<MultiRangeDto> _multiRanges = ShopService.GetMultiRangeList();
        readonly List<string> _categories = ShopService.GetCategortyList();
        readonly List<string> _brands = ShopService.GetBrandList();
        readonly ShopPage _shopData = new(ShopService.GetGoodsList());

        [Inject]
        public NavigationManager Nav { get; set; } = default!;

        protected override void OnInitialized()
        {
            _shopData.MultiRange = _multiRanges[0];
        }

        private void NavigateToDetails(Guid id)
        {
            Nav.NavigateTo($"app/ecommerce/details/{id}");
        }

        private void NavigateToOrder()
        {
            Nav.NavigateTo($"app/ecommerce/order");
        }
    }
}
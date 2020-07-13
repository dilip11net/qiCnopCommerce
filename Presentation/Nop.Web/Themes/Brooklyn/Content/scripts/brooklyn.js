(function ($, ssCore, ssEx) {

    window.themeSettings = {
        themeBreakpoint: 1024,
        isAccordionMenu: false
    }

    var previousWidth = ssCore.getViewPort().width;

    function handleHomePageSliders() {

        if (ssCore.getViewPort().width >= 768) {

            var mainSlider = $('.main-banner-1 .anywhere-slick-slider');
            var secondSlider = $('.main-banner-2 .anywhere-slick-slider');
            var mainSliderId = mainSlider.attr('id');
            var secondSliderId = secondSlider.attr('id');

            secondSlider.css('opacity', 0); // hiding the second slider until the first one is loaded to avoid layout shifting

            var incrementCounter = 0;
            var showSlider = function () {

                incrementCounter++;

                if (incrementCounter == 2) {
                    secondSlider.css('opacity', 1);
                }
            }
            $(document).on('AnywhereSliderFirstImageLoaded' + mainSliderId, showSlider);
            $(document).on('AnywhereSliderFirstImageLoaded' + secondSliderId, showSlider);
        }
    }
    handleHomePageSliders(); // invoke outside document.ready

    function resizeAndInitializeFlyoutCart() {
        var previousHeight = ssCore.getViewPort().height;
        var cartCountHeight = $('.mini-shopping-cart .count').height();
        var cartItemHeight = $('.mini-shopping-cart .items').height();
        var cartTotalHeight = $('.mini-shopping-cart .totals').height();
        var cartButtonsHeight = $('.mini-shopping-cart .buttons').height();
        var general = cartCountHeight + cartItemHeight + cartTotalHeight + cartButtonsHeight;

        if (general >= previousHeight) {
            $('.mini-shopping-cart .items').height(previousHeight - 250);
            $('.mini-shopping-cart .items').simplebar();

        }
    }

    function repositionLogoOnWidthBreakpointForHeader2(themeBreakpoint) {
        if ($('.header.header-2').length > 0) {
            if ($(window).outerWidth() > themeBreakpoint) {
                $('.header-logo').prependTo('.header-options-wrapper').after($('.store-search-box'));
            }
            else {
                $('.header-logo').prependTo('.header-actions-wrapper');
            }
        }
    }

    function toggleFooterBlocks(isInitialLoad, themeBreakpoint) {
        var viewport = ssCore.getViewPort().width;

        if (isInitialLoad || (viewport > themeBreakpoint && previousWidth <= themeBreakpoint) || (viewport <= themeBreakpoint && previousWidth > themeBreakpoint)) {
            if (viewport >= themeBreakpoint) {
                $('.footer-block .title').next('div, ul').slideDown();
            }
            else {
                $('.footer-block .title').next('div, ul').slideUp();
            }
        }

        previousWidth = viewport;
    }

    function initializeFlyoutCart(movedElementsSelector, themeBreakpoint) {
        var movedElements = $(movedElementsSelector);

        $('.header .ico-cart').on('click', function (e) {
            var flyoutCart = $('#flyout-cart');

            var flyoutCartCount = $('.flyout-cart .count');
            if (flyoutCartCount.find('.close').length === 0) {
                flyoutCartCount.append('<span class="close"></span>');
            }

            if (ssCore.getViewPort().width > themeBreakpoint) {
                e.preventDefault();

                if (flyoutCart.hasClass('active')) {
                    flyoutCart.removeClass('active');
                    movedElements.removeClass('move-left');
                } else {
                    flyoutCart.addClass('active');
                    movedElements.addClass('move-left');
                }
            }
        });

        $('.flyout-cart .count').append('<span class="close"></span>');

        $('.header').on('click', '#flyout-cart .close', function () {
            $('#flyout-cart').removeClass('active');
            movedElements.removeClass('move-left');
        });
    }
    
    function stretchElementsToBodyWidth(elements) {
        // make categories and blog posts width 100%
        var stretchedElements = $(elements);
        var neededHorizontalMargin = -(parseInt($('body').outerWidth()) * .05);

        stretchedElements.css({
            marginLeft: neededHorizontalMargin,
            marginRight: neededHorizontalMargin
        });
    }

    
    $(document).ready(function () {
        $(window).on('load resize orientationchange', function () {
            if (ssCore.getViewPort().width <= 480) {
                stretchElementsToBodyWidth('.home-page-category-grid, .sub-category-grid, .rich-blog-homepage .blog-posts, .gallery');
            }
            else {
                $('.home-page-category-grid, .sub-category-grid, .rich-blog-homepage .blog-posts, .gallery').css({ 'margin-left': 'auto', 'margin-right': 'auto' });
            }
        });

        var searchBoxBeforeSelector = $('.header-2').length > 0 ? ".header-logo" : ".header-options";

        var responsiveAppSettings = {
            isEnabled: true,
            themeBreakpoint: window.themeSettings.themeBreakpoint,
            isSearchBoxDetachable: true,
            isHeaderLinksWrapperDetachable: false,
            doesDesktopHeaderMenuStick: false,
            doesScrollAfterFiltration: true,
            doesSublistHasIndent: true,
            displayGoToTop: true,
            hasStickyNav: false,
            selectors: {
                menuTitle: ".menu-title",
                headerMenu: ".header-menu",
                closeMenu: ".close-menu span",
                sublist: ".header-menu .sublist",
                overlayOffCanvas: ".overlayOffCanvas",
                withSubcategories: ".with-subcategories",
                filtersContainer: ".nopAjaxFilters7Spikes",
                filtersOpener: ".filters-button span",
                searchBoxOpener: ".search-wrap > span",
                searchBox: ".store-search-box",
                searchBoxBefore: searchBoxBeforeSelector,
                navWrapper: ".responsive-nav-wrapper",
                navWrapperParent: ".responsive-nav-wrapper-parent",
                headerLinksOpener: "#header-links-opener",
                headerLinksWrapper: ".header-options-wrapper",
                shoppingCartLink: ".shopping-cart-link",
                overlayEffectDelay: 300
            }
        };

        ssEx.initResponsiveTheme(responsiveAppSettings);

        $(document).on("nopAjaxCartProductAddedToCartEvent nopAjaxCartFlyoutShoppingCartUpdated load", function () {
            resizeAndInitializeFlyoutCart();
        });

        resizeAndInitializeFlyoutCart();


        // click to close (search, header links and mobile menu )

        $('.responsive-nav-wrapper .search-wrap span').on('click', function () {
            if ($('.header-options-wrapper').hasClass('open')) {
                $('.header-options-wrapper').removeClass('open');
            }

            if ($('.header-menu').hasClass('open')) {
                $('.header-menu').removeClass('open');
            }
        });

        $('.responsive-nav-wrapper .personal-button span').on('click', function () {
            if ($('.store-search-box').hasClass('open')) {
                $('.store-search-box').removeClass('open');
            }

            if ($('.header-menu').hasClass('open')) {
                $('.header-menu').removeClass('open');
            }
        });

        $('.menu-title span').on('click', function () {
            if ($('.header-options-wrapper').hasClass('open')) {
                $('.header-options-wrapper').removeClass('open');
            }

            if ($('.store-search-box').hasClass('open')) {
                $('.store-search-box').removeClass('open');
            }
        });

        // footer slide up and down
        $('.footer-block > .title, .footer-1 .newsletter-block .newsletter .title').on('click', function () {
            if (ssCore.getViewPort().width < responsiveAppSettings.themeBreakpoint) {
                $(this).next('div, ul').slideToggle();
            }
        });

        ssEx.addWindowEvent('resize', function () {
            toggleFooterBlocks(false, responsiveAppSettings.themeBreakpoint);
            resizeAndInitializeFlyoutCart();
            repositionLogoOnWidthBreakpointForHeader2(responsiveAppSettings.themeBreakpoint);
        });
        ssEx.addWindowEvent('orientationchange', function () {
            toggleFooterBlocks(false, responsiveAppSettings.themeBreakpoint);
            resizeAndInitializeFlyoutCart();
            repositionLogoOnWidthBreakpointForHeader2(responsiveAppSettings.themeBreakpoint);
        });

        toggleFooterBlocks(true, responsiveAppSettings.themeBreakpoint);
        repositionLogoOnWidthBreakpointForHeader2(responsiveAppSettings.themeBreakpoint);

        initializeFlyoutCart(responsiveAppSettings.selectors.movedElements, responsiveAppSettings.themeBreakpoint);
    });


})(jQuery, sevenSpikesCore, sevenSpikesEx);
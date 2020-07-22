// JavaScript Document

/// graphic colors
var $colors = [
  /*0*/ "#2f6ebf",
  /*1*/ "#1445aa",
  /*2*/ "#2865ba",
  /*3*/ "#3a7fc8",
  /*4*/ "#6ba2d1",
  /*5*/ "#448fd0",
  /*6*/ "#c5ebfd", /// azul bebe
  /*7*/ "#f8b9f6" /// rosa
];

/// template colors
var $colorsTheme = [
  /*$light-blue: 0 */ "#61bce8",
		/*$blue: 1 */ "#042d9d",
	  /*$yellow: 2 */ "#ffd342",
	   /*$green: 3 */ "#2aa745",
		 /*$red: 4 */ "#ff0000",
  /*$white-gray: 5 */ "#fbfbfb",
	   /*$black: 6 */ "#1a1818",
		/*$gray: 7 */ "#777777",
  /*$light-gray: 8 */ "#f0f0f0"
];




function collapseNavbar() {

    var topo = $(".fixed-top");
    var meio = topo.innerHeight()/* * 10 / 100*/;

    if (topo.offset().top > meio) {
        $(".fixed-top nav.navbar").addClass("top-nav-collapse");
    } else {
        $(".fixed-top nav.navbar").removeClass("top-nav-collapse");
    }
}

var largura = $('body').innerWidth();
if (largura > 1199) {
	$(window).scroll(collapseNavbar);
	$(document).ready(collapseNavbar);
}

$(window).on('resize', function () {
	if (largura < 1199) {
		$(".fixed-top nav.navbar").removeClass("top-nav-collapse");
	}
	else {
		$(window).scroll(collapseNavbar);
		$(document).ready(collapseNavbar);
	}
});



$(function () {
    ///"use strict"; // Start of use strict

    
    /// custom select
    /// seta do select
    var selectC = $('select');
    var execSELECTC = false;
    if (selectC.length) { execSELECTC = true; }

    if (execSELECTC !== false) {
        /// adicionando uma seta
        $.each(selectC, function () {
            var findselect = $(this);

            if (!findselect.is('.selectpicker')) {
                findselect.closest('div').append('<i class="caret"></i>');
            }
        });
    }



    //// gallery filters
    var gallery = $('.g-filters');
    var execGF = false;
    /// verificando se exixte antes de executar
    if (gallery.length) { execGF = true; }

    /// execução do do hide filter
    if (execGF !== false) {
        var $fblock = $('.g-filters .filters-block');
        var $filterBtn = $('.g-filters .filters-block .btn.filters');
        var $filtersBar = $('.g-filters .filters-block .mobile-filter-bar');

        $($fblock).prepend('<span class="overlay"></span>');
        $filtersBar.prepend('<div class="close-content"><a title="" class="close-bar btn btn-gray-outline btn-shadow-none btn-square"><i class="fas fa-times"></i></a></div>');

        $($filterBtn).bind('click', function (e) {
            $($filtersBar).addClass('active');
            $($fblock).find('.overlay').addClass('active');

            e.preventDefault();
        });
        $('.close-bar, span.overlay').on('click', function (e) {
            $($filtersBar).removeClass('active');
            $($fblock).find('.overlay').removeClass('active');

            e.preventDefault();
        });

    }


    //// menu do dash --- temp até q seja revisada a utilização.
    var menuDash = $('.page-sidebar');
    var execMD = false;
    /// verificando se exixte antes de executar
    if (menuDash.length) { execMD = true; }

    /// execução do do hide filter
    if (execMD !== false) {
        var $menublock = $('.page-sidebar');

        $($menublock).prepend('<span class="overlay"></span>');

        $('.page-sidebar .overlay').on('click', function (e) {
            $menublock.removeClass('show');

            e.preventDefault();
        });
    }



    //// SELECT
    var select = $('.material-form select');
    var execSELECT = false;
    /// verificando se exixte antes de executar
    if (select.length) { execSELECT = true; }

    /// execução do SELECT
    if (execSELECT !== false) {

        /// adicionando uma seta
        ///select.closest('div').append('<i class="caret"></i>');

        /// verificando se exitem slects com valores
        $.each(select, function () {

            var myselect = $(this);
            var mylabel = myselect.siblings('label');
            var options = myselect.children('option:selected');
            var getNull = $('.first-label');

            if (options.is(getNull)) {
                mylabel.removeClass('up').addClass('down');
            } else {
                mylabel.removeClass('down').addClass('up');
            }
        });

        /// alterando no click
        ///$('.material-form select').bind('click',function(e){
        $('.material-form select').focus(function () {
            var selectClick = $(this);
            var closelabel = selectClick.siblings('label');

            closelabel.removeClass('down').addClass('up');
            ///event.stopPropagation();
        });

        $('.material-form select').focusout(function () {
            var selectClick = $(this);
            var closelabel = selectClick.siblings('label');
            var optionSt = selectClick.children('option:selected');
            var setEmpty = $('.first-label');

            if (optionSt.is(setEmpty)) {
                closelabel.removeClass('up').addClass('down');
            } else {
                /// do nothing
            }
        });


    }



    /***** STEPS TOGGLE **********/
    var steps = $('[data-step]');
    var execSteps = false;
    /// verificando se exixte antes de executar
    if (steps.length) { execSteps = true; }

    /// execução do SELECT
    if (execSteps !== false) {
        $('[data-toggle-step]').bind('click', function (e) {
            ///var stepChange = $(this).attr('[data-togle-step]');
            var btnClicked = $(this);
            var control = btnClicked.closest('[data-step]');
            var changeStep = btnClicked.data('toggle-step');


            /// rolagem de volta
            ///var largura = $('body, html').innerWidth();
            if (largura < 768) {
                var reduzTop = 98;
            } else { var reduzTop = 120; }

            var target = control;

            $('html, body').animate({
                scrollTop: target.offset().top - reduzTop
            }, 200);


            /// pqno atraso
            setTimeout(function () {
                control.attr('data-step', changeStep);
            }, 200);

            ///steps.toggle(stepChange);
            e.preventDefault();
        });
    }



    $('.selectpicker').selectpicker();

    /// options...
    $('.grid').masonry({
        columnWidth: '.grid-item',
        itemSelector: '.grid-item',
        //fitWidth: true,
        //gutter: 0,
        horizontalOrder: true,
        percentPosition: true
    });




    var execCSBar = false;

    var cus_bar = $('.customs-crollbar');
    var cat_bar = $('.categories');

    if (cus_bar.length || cat_bar.length) { execCSBar = true; }

    if (execCSBar !== false) {

        /// my custom scrollbar  --> http://manos.malihu.gr/jquery-custom-content-scroller/			
        $('.customs-crollbar').mCustomScrollbar({
            axis: 'y',
            scrollInertia: 150,
            autoExpandScrollbar: false,
            alwaysShowScrollbar: 0,
            theme: 'minimal-dark'
        });

        /// so para categorias
        $('.categories').mCustomScrollbar({
            axis: "x",
            ///scrollInertia:350,
            ///autoExpandScrollbar:true,
            contentTouchScroll: true,
            alwaysShowScrollbar: 0,
            documentTouchScroll: true,
            ///moveDragger:true,
            theme: 'dark-thin',
            scrollButtons: {
                enable: true,
                //scrollType: "stepless"
                scrollAmount: 350,
                scrollType: 'stepped',
            },
            callbacks: {
                whileScrolling: function () {
                    findArrows(this, $(this));
                },
                onTotalScrollBack: function () {
                    $(this).find('.mCSB_buttonLeft').fadeOut();
                },
                onTotalScroll: function () {
                    $(this).find('.mCSB_buttonRight').fadeOut();
                }
            },
            advanced: {
                autoExpandHorizontalScroll: true,
                ///extraDraggableSelectors: ".item"
            }
        });


        function findArrows(el, scroller) {

            var scrollPercentage = (el.mcs.leftPct);
            var leftArrow = scroller.find('.mCSB_buttonLeft');
            var rightArrow = scroller.find('.mCSB_buttonRight');

            /// seta esquerda
            if (scrollPercentage > 10) {
                leftArrow.fadeIn();
            } else {
                leftArrow.fadeOut();
            }

            /// seta direita
            if (scrollPercentage > 98) {
                rightArrow.fadeOut();
            } else {
                rightArrow.fadeIn();
            }

        }

    }




    var categories = $('.categories');
    var execcategories = false;

    /// verificando se exixte antes de executar
    if (categories.length) { execcategories = true; }

    /// random colors
    if (execcategories !== false) {

        var items = categories.find('.item');

        var backcolor = [
            '#af9c92',
            '#b3b8c2',
            '#abb3b6',
            '#a79e88',
            '#aba9a4',
            '#c1beb4',
            '#726c62',
            '#768346',
            '#af9d85',
            '#48464b',
            '#3a3837',
            '#927c69',
            '#c1bea8',
            '#c17f67',
            '#412433'
        ];

        $.each(items, function () {

            var ribbon = $(this);
            var rand = backcolor[Math.floor(Math.random() * backcolor.length)];

            ribbon.css('background', rand);

        });
    }



    //// filter brands btns
    $('.filter-brands [data-brand-filter]').on('click', function (e) {
        /// tratando o btn clicado
        var $filterShow = $(this).attr("data-brand-filter");

        $(this).addClass('active');
        $('.filter-brands  [data-brand-filter]').not(this).removeClass('active');

        /// tratando os elementos para exibição
        var hideClass = 'hide-brand';
        var brands = $('.brands-container div[data-type-filter]');
        var exibir = $('.brands-container div[data-type-filter*="' + $filterShow + '"]');

        if ($filterShow != 'all') {
            brands.addClass(hideClass);
            exibir.removeClass(hideClass);
        } else {
            brands.removeClass(hideClass);
        }

        e.preventDefault();
    });



    $('.parallax').each(function () {
        var $obj = $(this);
        var $dataSpeed = $obj.data('speed');
        ///var $limit = $obj.is(':visible');
        ///var $posAtual = $obj.offset().top;
        ///var $screen = $obj.height();

        if ($dataSpeed == null || $dataSpeed == ' ') {
            $dataSpeed = '10';
        }
        else { $dataSpeed = $obj.data('speed'); }

        $(window).scroll(function () {
            var offset = $obj.offset();
            var yPos = -($(window).scrollTop() - offset.top) / $dataSpeed;
            var bgpos = '50% ' + yPos + 'px';
            $obj.css('background-position', bgpos);
        });
    });





});



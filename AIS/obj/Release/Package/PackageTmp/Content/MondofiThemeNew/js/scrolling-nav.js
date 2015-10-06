//jQuery to collapse the navbar on scroll
$(window).scroll(function() {
    if ($("#navFirst").offset().top > 50) {
        $(".navbar-fixed-top").addClass("top-nav-collapse");
    } else {
        $(".navbar-fixed-top").removeClass("top-nav-collapse");
    }
});


//jQuery for page scrolling feature - requires jQuery Easing plugin
$(function() {
    $('a.page-scroll').bind('click', function(event) {
        var $anchor = $(this);
        $('html, body').stop().animate({
            scrollTop: $($anchor.attr('href')).offset().top
        }, 1500, 'easeInOutExpo');
        event.preventDefault();
    });
});

// Second Nav fixed
 var $window = $(window),
               $stickyEl = $('#the-sticky-div'),
               elTop = $stickyEl.offset().top;

           $window.scroll(function() {
                $stickyEl.toggleClass('sticky', (parseFloat($window.scrollTop()) + 20) > elTop);
});


//active menu

            $(document).ready(function () {
                $(document).on("scroll", onScroll);
                
                //smoothscroll
                $('a[href^="#"]').on('click', function (e) {
                    e.preventDefault();
                    $(document).off("scroll");
                    
                    $('a').each(function () {
                        $(this).removeClass('active');
                    })
                    $(this).addClass('active');
                  
                    var target = this.hash,
                        menu = target;
                    $target = $(target);
                    $('html, body').stop().animate({
                        'scrollTop': $target.offset().top + 2
                    }, 500, 'swing', function () {
                        //window.location.hash = target;
                        $(document).on("scroll", onScroll);
                    });
                });
            });

function onScroll(event){
    var scrollPos = $(document).scrollTop();
    $('.secondNav a').each(function () {
        var currLink = $(this);
        var refElement = $(currLink.attr("href"));
        if (refElement.position().top <= scrollPos && refElement.position().top + refElement.height() > scrollPos) {
            $('#menu-center ul li a').removeClass("active");
            currLink.addClass("active");
        }
        else{
            currLink.removeClass("active");
        }
    });
}

// click on logo
$(document).ready(function(){
    $('.navbar-brand.actvie').on('click', function(){
        $('html,body').animate({scrollTop: $(this).offset().top}, 800);
    }); 
});  
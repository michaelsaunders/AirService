 $(document).ready(function(){
   $.supersized({
		autoplay: 0,
		//slide_interval	:   5300,	
		transition: 1, 	
		transition_speed: 700,
		slide_links: 'false',	
		keyboard_nav: 0, 
		slides	:	[
						{image : 'Content/images/common/vegas_bg.jpg'},
						{image : 'Content/images/common/hotel_bg.jpg'},
						{image : 'Content/images/common/holiday_bg.jpg'},
						{image : 'Content/images/common/photos_bg.jpg'},						
						{image : 'Content/images/common/timed_bg.jpg'},
						{image : 'Content/images/common/beer_bg.jpg'},
						{image : 'Content/images/common/bar_bg.jpg'}						
					]
		
	});
	
	$(".slides-container").jCarouselLite({
        btnNext: "#nextslide",
        btnPrev: "#prevslide",
		circular: true,
		//auto: 5300,
		speed: 700,
		visible: 1
    });
});

// Fix for background image positioning.
var myW = $("#supersized img").width() - $(document).width();
myW /= 2;
$("#supersized img").css("left", (0 - myW));
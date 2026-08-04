/**
 * @name video-iframe
 * @function handle youtube video iframe
 */

// 3. This function creates an <iframe> (and YouTube player)
//    after the API code downloads.
var tag = document.createElement('script');

tag.src = "https://www.youtube.com/iframe_api";
var firstScriptTag = document.getElementsByTagName('script')[0];
firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);

document.addEventListener('DOMContentLoaded', function() {
  var player;
  function onYouTubeIframeAPIReady(id) {
    player = new YT.Player('video_iframe', {
      height: '360',
      width: '640',
      videoId: id,
      playerVars : {
        autoplay: 1,
        rel: 0,
      },
      events: {
        'onReady': onPlayerReady,
        'onStateChange': onPlayerStateChange
      }
    });
  }

  function onPlayerReady(event) {
    // event.target.playVideo();
  }

  var done = false;
  function onPlayerStateChange(event) {
    if (event.data == YT.PlayerState.PLAYING && !done) {
      // setTimeout(stopVideo, 6000);
      done = true;
    }
  }
  function playVideo() {
    player.playVideo();
  }

  function stopVideo() {
    player.stopVideo();
    $('#video_modal .text-center').append("<div id='video_iframe'></div>");
    $('iframe#video_iframe').remove();
  }

  var videoModal = document.getElementById('video_modal');
  var instances = M.Modal.init(videoModal, {
    onOpenEnd: function(e, btn) {
      var videoId = btn.getAttribute("data-video");
      onYouTubeIframeAPIReady(videoId);
    },
    onCloseEnd: function() {
      stopVideo();
    }
  });
  $('.modal .modal-close').click(function(){
    stopVideo();
  })
});
/******** END Video Iframe ********/


/**
 * @name blog carousel
 * @function handle carousel blog
 */

$(function() {
  var $carousel = $('#blog_carousel');
  // Handle carousel
  $carousel.slick({
    dots: true,
    infinite: true,
    autoplay: true,
    autoplaySpeed: 5000,
    speed: 750,
    fade: true,
    arrows: false
  });
});
/******** END Video carousel ********/
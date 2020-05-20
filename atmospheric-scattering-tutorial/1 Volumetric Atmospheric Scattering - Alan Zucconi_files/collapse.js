/*!
* Collapse-O-Matic JavaSctipt v1.6.18
* http://plugins.twinpictures.de/plugins/collapse-o-matic/
*
* Copyright 2019, Twinpictures
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, blend, trade,
* bake, hack, scramble, difiburlate, digest and/or sell copies of the Software,
* and to permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/function collapse_init(){jQuery('.force_content_collapse').each(function(index){jQuery(this).css('display','none');});jQuery('.collapseomatic:not(.colomat-close)').each(function(index){var thisid=jQuery(this).attr('id');jQuery('#target-'+thisid).css('display','none');});jQuery('.collapseomatic.colomat-close').each(function(index){var thisid=jQuery(this).attr('id');if(jQuery("#swap-"+thisid).length>0){swapTitle(this,"#swap-"+thisid);}
if(jQuery("#swapexcerpt-"+thisid).length>0){swapTitle("#excerpt-"+thisid,"#swapexcerpt-"+thisid);}
jQuery('[id^=extra][id$='+thisid+']').each(function(index){if(jQuery(this).data('swaptitle')){old_swap_title=jQuery(this).data('swaptitle');old_title=jQuery(this).html();jQuery(this).html(old_swap_title);jQuery(this).data('swaptitle',old_title);}});});}
function swapTitle(origObj,swapObj){if(jQuery(origObj).prop("tagName")=='IMG'){var origsrc=jQuery(origObj).prop('src');var swapsrc=jQuery(swapObj).prop('src');jQuery(origObj).prop('src',swapsrc);jQuery(swapObj).prop('src',origsrc);}
else{var orightml=jQuery(origObj).html();var swaphtml=jQuery(swapObj).html();jQuery(origObj).html(swaphtml);jQuery(swapObj).html(orightml);if(swaphtml.indexOf("<cufon")!=-1){var trigelem=jQuery(this).get(0).tagName;Cufon.replace(trigelem);}}}
function toggleState(obj,id,maptastic,trig_id){if(maptastic&&jQuery('[id^=target][id$='+id+']').hasClass('maptastic')){jQuery('[id^=target][id$='+id+']').removeClass('maptastic');}
com_effect=colomatslideEffect;com_duration=colomatduration;if(obj.attr('data-animation_effect')){com_effect=obj.attr('data-animation_effect');}
if(obj.attr('data-duration')){com_duration=obj.attr('data-duration');}
if(isFinite(com_duration)){com_duration=parseFloat(com_duration);}
if(com_effect=='slideToggle'){jQuery('#target-'+id).slideToggle(com_duration,function(){if(jQuery(this).hasClass('colomat-inline')&&jQuery(this).is(':visible')){jQuery(this).css('display','inline');}
if(trig_id&&jQuery('#'+trig_id).is('.find-me.colomat-close')){offset_top=jQuery('#'+trig_id).attr('data-findme');if(!offset_top||offset_top=='auto'){target_offset=jQuery('#'+trig_id).offset();offset_top=target_offset.top;}
jQuery('html, body').animate({scrollTop:offset_top},500);}});}
else if(com_effect=='slideFade'){jQuery('#target-'+id).animate({height:"toggle",opacity:"toggle"},com_duration,function(){if(jQuery(this).hasClass('colomat-inline')&&jQuery(this).is(':visible')){jQuery(this).css('display','inline');}
if(trig_id&&jQuery('#'+trig_id).is('.find-me.colomat-close')){offset_top=jQuery('#'+trig_id).attr('data-findme');if(!offset_top||offset_top=='auto'){target_offset=jQuery('#'+trig_id).offset();offset_top=target_offset.top;}
jQuery('html, body').animate({scrollTop:offset_top},500);}});}
if(jQuery('#'+id).hasClass('colomat-close')){jQuery('.google-maps-builder').each(function(index){map=jQuery(".google-maps-builder")[index];google.maps.event.trigger(map,'resize');});}
if(typeof colomat_callback!='undefined'){colomat_callback();}}
function closeOtherGroups(rel){jQuery('.collapseomatic[rel!="'+rel+'"]').each(function(index){if(jQuery(this).hasClass('colomat-expand-only')&&jQuery(this).hasClass('colomat-close')){return;}
if(jQuery(this).hasClass('colomat-close')&&jQuery(this).attr('rel')!==undefined){jQuery(this).removeClass('colomat-close');var id=jQuery(this).attr('id');jQuery('#parent-'+id).removeClass('colomat-parent-highlight');if(jQuery("#swap-"+id).length>0){swapTitle(this,"#swap-"+id);}
if(jQuery("#swapexcerpt-"+id).length>0){swapTitle("#exerpt-"+id,"#swapexcerpt-"+id);}
jQuery('[id^=extra][id$='+id+']').each(function(index){if(jQuery(this).data('swaptitle')){old_swap_title=jQuery(this).data('swaptitle');old_title=jQuery(this).html();jQuery(this).html(old_swap_title);jQuery(this).data('swaptitle',old_title);}});toggleState(jQuery(this),id,false,false);var ancestors=jQuery('.collapseomatic','#target-'+id);ancestors.each(function(index){jQuery(this).removeClass('colomat-close');var thisid=jQuery(this).attr('id');jQuery('#target-'+thisid).css('display','none');})}});}
function closeOtherMembers(rel,id){jQuery('.collapseomatic[rel="'+rel+'"]').each(function(index){if(jQuery(this).hasClass('colomat-expand-only')&&jQuery(this).hasClass('colomat-close')){return;}
if(jQuery(this).attr('id')!=id&&jQuery(this).hasClass('colomat-close')&&jQuery(this).attr('rel')!==undefined){jQuery(this).removeClass('colomat-close');var thisid=jQuery(this).attr('id');jQuery('#parent-'+thisid).removeClass('colomat-parent-highlight');if(jQuery("#swap-"+thisid).length>0){swapTitle(this,"#swap-"+thisid);}
if(jQuery("#swapexcerpt-"+thisid).length>0){swapTitle("#excerpt-"+thisid,"#swapexcerpt-"+thisid);}
jQuery('[id^=extra][id$='+thisid+']').each(function(index){if(jQuery(this).data('swaptitle')){old_swap_title=jQuery(this).data('swaptitle');old_title=jQuery(this).html();jQuery(this).html(old_swap_title);jQuery(this).data('swaptitle',old_title);}});if(!jQuery(this).hasClass('colomat-close')&&jQuery(this).hasClass('snap-shut')){jQuery('#target-'+thisid).hide();}
else{toggleState(jQuery(this),thisid,false,false);}
var ancestors=jQuery('.collapseomatic','#target-'+id);ancestors.each(function(index){if(jQuery(this).hasClass('colomat-expand-only')&&jQuery(this).hasClass('colomat-close')){return;}
var pre_id=id.split('-');if(pre_id[0].indexOf('extra')!='-1'){pre=pre_id.splice(0,1);id=pre_id.join('-');if(jQuery(this).hasClass('scroll-to-trigger')){var target_offset=jQuery('#'+id).offset();offset_top=target_offset.top;}
if(jQuery('#'+id).hasClass('scroll-to-trigger')){offset_top=jQuery('#scrollonclose-'+id).attr('name');if(offset_top=='auto'){var target_offset=jQuery('#'+id).offset();offset_top=target_offset.top;}}
jQuery('#'+id).toggleClass('colomat-close');jQuery('[id^=extra][id$='+id+']').toggleClass('colomat-close');}
if(jQuery(this).attr('id').indexOf('bot-')=='-1'){jQuery(this).removeClass('colomat-close');var thisid=jQuery(this).attr('id');if(jQuery("#swap-"+thisid).length>0){swapTitle(this,"#swap-"+thisid);}
if(jQuery("#swapexcerpt-"+thisid).length>0){swapTitle("#excerpt-"+thisid,"#swapexcerpt-"+thisid);}
jQuery('[id^=extra][id$='+thisid+']').each(function(index){if(jQuery(this).data('swaptitle')){old_swap_title=jQuery(this).data('swaptitle');old_title=jQuery(this).html();jQuery(this).html(old_swap_title);jQuery(this).data('swaptitle',old_title);}});jQuery('#target-'+thisid).css('display','none');}})}});}
function colomat_expandall(loop_items){if(!loop_items){loop_items=jQuery('.collapseomatic:not(.colomat-close)');}
loop_items.each(function(index){jQuery(this).addClass('colomat-close');var thisid=jQuery(this).attr('id');jQuery('#parent-'+thisid).addClass('colomat-parent-highlight');if(jQuery("#swap-"+thisid).length>0){swapTitle(this,"#swap-"+thisid);}
if(jQuery("#swapexcerpt-"+thisid).length>0){swapTitle("#excerpt-"+thisid,"#swapexcerpt-"+thisid);}
jQuery('[id^=extra][id$='+thisid+']').each(function(index){if(jQuery(this).data('swaptitle')){old_swap_title=jQuery(this).data('swaptitle');old_title=jQuery(this).html();jQuery(this).html(old_swap_title);jQuery(this).data('swaptitle',old_title);}});toggleState(jQuery(this),thisid,false,false);});}
function colomat_collapseall(loop_items){if(!loop_items){loop_items=jQuery('.collapseomatic.colomat-close');}
loop_items.each(function(index){if(jQuery(this).hasClass('colomat-expand-only')&&jQuery(this).hasClass('colomat-close')){return;}
jQuery(this).removeClass('colomat-close');var thisid=jQuery(this).attr('id');jQuery('#parent-'+thisid).removeClass('colomat-parent-highlight');if(jQuery("#swap-"+thisid).length>0){swapTitle(this,"#swap-"+thisid);}
if(jQuery("#swapexcerpt-"+thisid).length>0){swapTitle("#excerpt-"+thisid,"#swapexcerpt-"+thisid);}
jQuery('[id^=extra][id$='+thisid+']').each(function(index){if(jQuery(this).data('swaptitle')){old_swap_title=jQuery(this).data('swaptitle');old_title=jQuery(this).html();jQuery(this).html(old_swap_title);jQuery(this).data('swaptitle',old_title);}});toggleState(jQuery(this),thisid,false,false);});}
jQuery(document).ready(function(){com_binding='click';if(typeof colomattouchstart!=='undefined'&&colomattouchstart){com_binding='click touchstart';}
if(typeof colomatpauseInit!=='undefined'&&colomatpauseInit){init_pause=setTimeout(collapse_init,colomatpauseInit);}
else{collapse_init();}
jQuery(document.body).on('post-load',function(){collapse_init();});jQuery('.content_collapse_wrapper').each(function(index){jQuery(this).css('display','inline');});jQuery(document).on({mouseenter:function(){jQuery(this).addClass('colomat-hover');},mouseleave:function(){jQuery(this).removeClass('colomat-hover');},focusin:function(){jQuery(this).addClass('colomat-hover');},focusout:function(){jQuery(this).removeClass('colomat-hover');}},'.collapseomatic');jQuery(document).on('keypress','.collapseomatic',function(event){if(event.which==13){event.currentTarget.click();};});jQuery(document.body).on(com_binding,'.collapseomatic',function(event){var offset_top;if(jQuery(this).hasClass('colomat-expand-only')&&jQuery(this).hasClass('colomat-close')){return;}
if(jQuery(this).attr('rel')&&jQuery(this).attr('rel').indexOf('-highlander')!='-1'&&jQuery(this).hasClass('must-be-one')&&jQuery(this).hasClass('colomat-close')){return;}
var id=jQuery(this).attr('id');if(jQuery(this).hasClass('colomat-close')&&jQuery(this).hasClass('scroll-to-trigger')){offset_top=jQuery('#scrollonclose-'+id).attr('name');if(offset_top=='auto'){var target_offset=jQuery('#'+id).offset();offset_top=target_offset.top;}}
var id_arr=id.split('-');if(id_arr[0].indexOf('extra')!='-1'){pre=id_arr.splice(0,1);id=id_arr.join('-');if(jQuery(this).hasClass('scroll-to-trigger')){var target_offset=jQuery('#'+id).offset();offset_top=target_offset.top;}
if(jQuery('#'+id).hasClass('scroll-to-trigger')){offset_top=jQuery('#scrollonclose-'+id).attr('name');if(offset_top=='auto'){var target_offset=jQuery('#'+id).offset();offset_top=target_offset.top;}}
jQuery('#'+id).toggleClass('colomat-close');jQuery('[id^=extra][id$='+id+']').toggleClass('colomat-close');}
else if(id.indexOf('bot-')!='-1'){id=id.substr(4);jQuery('#'+id).toggleClass('colomat-close');if(jQuery(this).hasClass('scroll-to-trigger')){var target_offset=jQuery('#'+id).offset();offset_top=target_offset.top;}
if(jQuery('#'+id).hasClass('scroll-to-trigger')){offset_top=jQuery('#scrollonclose-'+id).attr('name');if(offset_top=='auto'){var target_offset=jQuery('#'+id).offset();offset_top=target_offset.top;}}}
else{jQuery(this).toggleClass('colomat-close');jQuery('[id^=extra][id$='+id+']').toggleClass('colomat-close');}
if(jQuery("#swap-"+id).length>0){swapTitle(jQuery('#'+id),"#swap-"+id);}
if(jQuery("#swapexcerpt-"+id).length>0){swapTitle("#excerpt-"+id,"#swapexcerpt-"+id);}
jQuery('[id^=extra][id$='+id+']').each(function(index){if(jQuery(this).data('swaptitle')){old_swap_title=jQuery(this).data('swaptitle');old_title=jQuery(this).html();jQuery(this).html(old_swap_title);jQuery(this).data('swaptitle',old_title);}});jQuery(this).addClass('colomat-visited');var parentID='parent-'+id;jQuery('#'+parentID).toggleClass('colomat-parent-highlight');if(!jQuery(this).hasClass('colomat-close')&&jQuery(this).hasClass('snap-shut')){jQuery('#target-'+id).hide();}
else{toggleState(jQuery(this),id,true,id);}
if(jQuery(this).attr('rel')!==undefined){var rel=jQuery(this).attr('rel');if(rel.indexOf('-highlander')!='-1'){closeOtherMembers(rel,id);}
else{closeOtherGroups(rel);}}
if(offset_top){jQuery('html, body').animate({scrollTop:offset_top},500);}});jQuery(document).on(com_binding,'.expandall',function(event){if(jQuery(this).attr('rel')!==undefined){var rel=jQuery(this).attr('rel');var loop_items=jQuery('.collapseomatic:not(.colomat-close)[rel="'+rel+'"]');}
else if(jQuery(this).attr('data-togglegroup')!==undefined){var toggroup=jQuery(this).attr('data-togglegroup');var loop_items=jQuery('.collapseomatic:not(.colomat-close)[data-togglegroup="'+toggroup+'"]');}
else{var loop_items=jQuery('.collapseomatic:not(.colomat-close)');}
colomat_expandall(loop_items);});jQuery(document).on(com_binding,'.collapseall',function(event){if(jQuery(this).attr('rel')!==undefined){var rel=jQuery(this).attr('rel');var loop_items=jQuery('.collapseomatic.colomat-close[rel="'+rel+'"]');}
else if(jQuery(this).attr('data-togglegroup')!==undefined){var toggroup=jQuery(this).attr('data-togglegroup');var loop_items=jQuery('.collapseomatic.colomat-close[data-togglegroup="'+toggroup+'"]');}
else{var loop_items=jQuery('.collapseomatic.colomat-close');}
colomat_collapseall(loop_items);});var fullurl=document.location.toString();if(fullurl.match('#(?!\!)')){hashmaster(fullurl);}
jQuery(document).on('click','a.colomat-nolink',function(event){event.preventDefault();});jQuery(window).on('hashchange',function(e){fullurl=document.location.toString();if(fullurl.match('#(?!\!)')){hashmaster(fullurl);}});function hashmaster(fullurl){if(fullurl.match('#(?!\!)')){var anchor_arr=fullurl.split(/#(?!\!)/);if(anchor_arr.length>1){junk=anchor_arr.splice(0,1);anchor=anchor_arr.join('#');}
else{anchor=anchor_arr[0];}
if(jQuery('#'+anchor).length){jQuery('#'+anchor).parents('.collapseomatic_content').each(function(index){parent_arr=jQuery(this).attr('id').split('-');junk=parent_arr.splice(0,1);parent=parent_arr.join('-');if(!jQuery('#'+parent).hasClass('colomat-close')){jQuery('#'+parent).click();}})
if(!jQuery('#'+anchor).hasClass('colomat-close')){jQuery('#'+anchor).click();}}
if(typeof colomatoffset!=='undefined'){var anchor_offset=jQuery('#'+anchor).offset();colomatoffset=colomatoffset+anchor_offset.top;jQuery('html, body').animate({scrollTop:colomatoffset},50);}}}});
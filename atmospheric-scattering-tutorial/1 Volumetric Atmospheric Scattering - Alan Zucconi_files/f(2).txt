(function(){/* 
 
 Copyright The Closure Library Authors. 
 SPDX-License-Identifier: Apache-2.0 
*/ 
'use strict';function f(a,c,d){a.addEventListener&&a.addEventListener(c,d,!1)};function g(a,c,d=null){h(a,c,d)}function h(a,c,d){a.google_image_requests||(a.google_image_requests=[]);const b=a.document.createElement("img");if(d){const e=r=>{d&&d(r);b.removeEventListener&&b.removeEventListener("load",e,!1);b.removeEventListener&&b.removeEventListener("error",e,!1)};f(b,"load",e);f(b,"error",e)}b.src=c;a.google_image_requests.push(b)};var k=(a=null)=>a&&22==a.getAttribute("data-jc")?a:document.querySelector('[data-jc="22"]');var l=document,m=window;function n(a,c,d){if(Array.isArray(c))for(var b=0;b<c.length;b++)n(a,String(c[b]),d);else null!=c&&d.push(a+(""===c?"":"="+encodeURIComponent(String(c))))};/* 
 Copyright (c) Microsoft Corporation. All rights reserved. 
 Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
 this file except in compliance with the License. You may obtain a copy of the 
 License at http://www.apache.org/licenses/LICENSE-2.0 
 
 THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY 
 KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED 
 WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
 MERCHANTABLITY OR NON-INFRINGEMENT. 
 
 See the Apache Version 2.0 License for specific language governing permissions 
 and limitations under the License. 
*/ 
function p(a){f(l,a.f,()=>{if(l[a.a])a.b&&(a.b=!1,a.c=Date.now(),q(a,0));else{if(-1!=a.c){const c=Date.now()-a.c;0<c&&(a.c=-1,q(a,1,c))}q(a,3)}})}function t(a){f(m,"click",c=>a.handleClick(c))} 
function q(a,c,d){var b={gqid:a.h,qqid:a.i};0==c&&(b["return"]=0);1==c&&(b["return"]=1,b.timeDelta=d);2==c&&(b.bgload=1);3==c&&(b.fg=1);c=[];for(var e in b)n(e,b[e],c);g(m,a.g+"&label=window_focus&"+c.join("&"),void 0);if(!(.01<Math.random())){a=(a=k(document.currentScript))&&a.getAttribute("data-jc-version")||"unknown";a=`https://${"pagead2.googleadservices.com"}/pagead/gen_204?id=jca&jc=${22}&version=${a}&sample=${.01}`;b=window;if(e=b.navigator)e=b.navigator.userAgent,e=/Chrome/.test(e)&&!/Edge/.test(e)? 
!0:!1;e&&b.navigator.sendBeacon?b.navigator.sendBeacon(a):g(b,a)}} 
class u{constructor(a,c,d){"undefined"!==typeof l.hidden?(this.a="hidden",this.f="visibilitychange"):"undefined"!==typeof l.mozHidden?(this.a="mozHidden",this.f="mozvisibilitychange"):"undefined"!==typeof l.msHidden?(this.a="msHidden",this.f="msvisibilitychange"):"undefined"!==typeof l.webkitHidden&&(this.a="webkitHidden",this.f="webkitvisibilitychange");this.g=a;this.b=!1;this.c=-1;this.h=c;this.i=d;l[this.a]&&q(this,2);p(this);t(this)}handleClick(){this.b=!0;m.setTimeout(()=>{this.b=!1},5E3)}};{const a=k(document.currentScript);if(null==a)throw Error("JSC not found 22");var v;{const c={},d=a.attributes;for(let b=d.length-1;0<=b;b--){const e=d[b].name;0===e.indexOf("data-jcp-")&&(c[e.substring(9)]=d[b].value)}v=c}window.window_focus_for_click=new u(v.url,v["gws-id"],v["qem-id"])};}).call(this);

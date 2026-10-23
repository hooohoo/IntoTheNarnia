const fs=require('fs'),path=require('path'),{spawn}=require('child_process');
const root=path.resolve(__dirname,'..'),base=fs.mkdtempSync(path.join(require('os').tmpdir(),'narnia-connected-browser-')),sleep=ms=>new Promise(r=>setTimeout(r,ms));
async function main(){
 const profile=path.join(base,'browser-profile'),shots=path.join(base,'Browser');fs.mkdirSync(profile,{recursive:true});fs.mkdirSync(shots,{recursive:true});const active=path.join(profile,'DevToolsActivePort');if(fs.existsSync(active))fs.unlinkSync(active);
 const child=spawn('/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',['--headless=new','--no-first-run','--disable-background-networking','--disable-extensions','--disable-sync','--remote-debugging-port=0','--remote-allow-origins=*','--user-data-dir='+profile,'about:blank'],{stdio:'ignore'});let ws;
 try{
  for(let i=0;i<150&&!fs.existsSync(active);i++)await sleep(100);if(!fs.existsSync(active))throw Error('Chrome did not start');
  const port=fs.readFileSync(active,'utf8').split('\n')[0],tabs=await(await fetch('http://127.0.0.1:'+port+'/json/list')).json();ws=new WebSocket(tabs.find(t=>t.type==='page').webSocketDebuggerUrl);await new Promise((r,j)=>{ws.onopen=r;ws.onerror=j;});
  let sequence=0;const pending=new Map(),errors=[],checks=[];ws.onmessage=e=>{const m=JSON.parse(e.data);if(m.id){const p=pending.get(m.id);pending.delete(m.id);m.error?p.reject(Error(JSON.stringify(m.error))):p.resolve(m.result);}else if(m.method==='Runtime.exceptionThrown')errors.push(m.params.exceptionDetails.text);};
  const send=(method,params={})=>new Promise((resolve,reject)=>{const id=++sequence;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}));});
  const evaluate=async expression=>{const r=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});if(r.exceptionDetails)throw Error(JSON.stringify(r.exceptionDetails));return r.result.value;};
  const snap=async name=>{const s=await send('Page.captureScreenshot',{format:'png'});fs.writeFileSync(path.join(shots,name+'.png'),Buffer.from(s.data,'base64'));};
  const action=async code=>{await evaluate(code);await sleep(100);const s=await evaluate('window.REVIEW_STATE');if(!s||s.webglError||s.visibleParts<1)throw Error('Bad render state '+JSON.stringify(s));return s;};
  const ready=async predicate=>{for(let i=0;i<600;i++){if(await evaluate(predicate))return;await sleep(100);}throw Error('Page ready timeout');};
  await send('Page.enable');await send('Runtime.enable');await send('Emulation.setDeviceMetricsOverride',{width:1440,height:1080,deviceScaleFactor:1,mobile:false});
  await send('Page.navigate',{url:'file://'+path.join(root,'Viewer.html')});await ready('!!window.REVIEW_STATE');
  let s=await evaluate('REVIEW_STATE');if(s.roomCount!==50||s.mode!=='exploded')throw Error('Initial model mismatch');checks.push('initial model / 50 rooms / offline WebGL');await snap('01-exploded');
  for(const mode of ['stacked','ground','upper','attic','exploded']){s=await action(`document.querySelector('[data-mode="${mode}"]').click()`);if(s.mode!==mode)throw Error('Mode '+mode);checks.push('mode '+mode);}
  for(const route of ['arrival','hide','escape']){
   s=await action(`document.getElementById('route').value='${route}';document.getElementById('route').dispatchEvent(new Event('change'))`);if(s.route!==route)throw Error('Route '+route);checks.push('route '+route);
   for(const progress of [0,.25,.5,.75,1]){s=await action(`document.getElementById('progress').value='${progress}';document.getElementById('progress').dispatchEvent(new Event('input'))`);if(!s.following||Math.abs(s.progress-progress)>.001)throw Error('Follow '+route+'/'+progress);checks.push('path preview '+route+' '+progress);}
   s=await action("document.getElementById('progress').value='.3';document.getElementById('progress').dispatchEvent(new Event('input'));document.getElementById('play').click()");await sleep(350);s=await action('void 0');if(!s.playing||s.progress<=.3)throw Error('Path playback');checks.push('playback '+route);await action("document.getElementById('play').click();document.getElementById('follow').click()");
  }
  for(const preset of ['hall','professor','children','wardrobe','garden','stairs']){s=await action(`document.querySelector('[data-preset="${preset}"]').click()`);if(s.preset!==preset)throw Error('Preset '+preset);checks.push('preset '+preset);if(['hall','stairs'].includes(preset))await snap('preset-'+preset);}
  const ids=await evaluate('HOUSE_MODEL.rooms.map(r=>r.id)');for(const id of ids){s=await action(`document.getElementById('room').value=${JSON.stringify(id)};document.getElementById('room').dispatchEvent(new Event('change'))`);if(s.selected!==id)throw Error('Room selection '+id);}checks.push('all 50 room selectors');
  for(const id of ['cutaway','labels','proxy']){await action(`document.getElementById('${id}').click()`);await action(`document.getElementById('${id}').click()`);checks.push('toggle '+id);}
  await action("document.getElementById('reset').click();document.querySelector('[data-mode=upper]').click()");await snap('02-upper');
  await action("document.getElementById('route').value='arrival';document.getElementById('route').dispatchEvent(new Event('change'));document.getElementById('progress').value='.025';document.getElementById('progress').dispatchEvent(new Event('input'))");await snap('03-eye-level');
  await send('Emulation.setDeviceMetricsOverride',{width:430,height:932,deviceScaleFactor:1,mobile:false});await action("document.getElementById('reset').click();scrollTo(0,0)");if(await evaluate('document.documentElement.scrollWidth>innerWidth'))throw Error('Mobile viewer overflow');checks.push('mobile viewer no overflow');await snap('04-mobile-viewer');
  await send('Emulation.setDeviceMetricsOverride',{width:1440,height:1080,deviceScaleFactor:1,mobile:false});await send('Page.navigate',{url:'file://'+path.join(root,'index.html')});await ready("document.readyState==='complete'&&!!window.STORY_STATE");
  const badImages=await evaluate('Array.from(document.images).filter(i=>!i.complete||!i.naturalWidth).map(i=>i.src)');if(badImages.length)throw Error('Missing images '+JSON.stringify(badImages));checks.push('all review images load');await snap('05-index');
  for(const story of ['arrival','hide','escape']){await evaluate(`document.querySelector('[data-story="${story}"]').click()`);if(await evaluate('STORY_STATE')!==story)throw Error('Story tab '+story);checks.push('review story '+story);}
  await send('Emulation.setDeviceMetricsOverride',{width:430,height:932,deviceScaleFactor:1,mobile:false});if(await evaluate('document.documentElement.scrollWidth>innerWidth'))throw Error('Mobile index overflow');checks.push('mobile review no overflow');await snap('06-mobile-index');
  if(errors.length)throw Error(JSON.stringify(errors));fs.writeFileSync(path.join(root,'Data/browser-checks.json'),JSON.stringify({passed:true,checks,errors,scope:'Offline local Chrome; controls, WebGL, path playback, image loading and desktop/mobile layout.'},null,2));console.log(JSON.stringify({passed:checks.length,errors:errors.length,roomSelections:ids.length}));
 }finally{if(ws)ws.close();child.kill('SIGTERM');}
}
main().catch(e=>{console.error(e);process.exitCode=1;});

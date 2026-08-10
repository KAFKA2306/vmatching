const listings = [
  {id:'cyber',title:'Cyber City でまったり雑談しよう！',kind:'雑談・交流',platform:'PC',current:6,capacity:10,time:'now',when:'今すぐ',lang:'JP',host:'Haru',description:'夜景が綺麗なワールドでゆっくりお話しませんか？ 初心者さんも大歓迎です！',tags:['初心者歓迎','雑談','まったり'],art:'city'},
  {id:'forest',title:'森のワールド探索🌲 のんびり散策',kind:'ワールド探索',platform:'Quest',current:3,capacity:8,time:'today',when:'今日 21:00〜',lang:'JP',host:'Moca',description:'自然の中を一緒にお散歩しましょう。写真撮影もOKです。',tags:['ワールド巡り','初心者歓迎'],art:'forest'},
  {id:'pavlov',title:'Pavlov VR でチームデスマッチ！',kind:'ゲーム',platform:'PC',current:8,capacity:10,time:'now',when:'今すぐ',lang:'JP/EN',host:'Ryu',description:'VCありでわいわい対戦しましょう！ 初心者でもOK。',tags:['ゲーム','VCあり'],art:'game'},
  {id:'sunset',title:'夕日を見ながらまったり撮影会📷',kind:'撮影',platform:'PC',current:4,capacity:12,time:'later',when:'明日 20:00〜',lang:'JP',host:'Sora',description:'綺麗な夕日スポットで自由に撮影しましょう。ポーズ集も用意しています。',tags:['撮影会','まったり'],art:'photo'}
];

const state = {query:'',size:'medium',time:'now',sort:'recommended',tab:'home',saved:new Set(),joined:new Set()};
const $ = (selector,root=document)=>root.querySelector(selector);
const $$ = (selector,root=document)=>[...root.querySelectorAll(selector)];
const container = $('#listingContainer');
const summary = $('#resultSummary');
const empty = $('#emptyState');

function sizeBucket(item){
  if(item.capacity<=3)return 'small';
  if(item.capacity<=10)return 'medium';
  return 'large';
}

function activeValues(name){
  return $$(`input[name="${name}"]:checked`).map(el=>el.value);
}

function matches(item){
  const kinds=activeValues('kind');
  const platforms=activeValues('platform');
  const haystack=[item.title,item.kind,item.platform,item.host,item.description,...item.tags].join(' ').toLowerCase();
  const query=state.query.trim().toLowerCase();
  if(query && !haystack.includes(query))return false;
  if(kinds.length && !kinds.includes(item.kind))return false;
  if(platforms.length && !platforms.includes(item.platform))return false;
  if(state.size && sizeBucket(item)!==state.size)return false;
  if(state.time && item.time!==state.time)return false;
  if(state.tab==='saved' && !state.saved.has(item.id))return false;
  if(state.tab==='planned' && !state.joined.has(item.id))return false;
  return true;
}

function ordered(items){
  const copy=[...items];
  if(state.sort==='open')copy.sort((a,b)=>(b.capacity-b.current)-(a.capacity-a.current));
  if(state.sort==='soon')copy.sort((a,b)=>({now:0,today:1,later:2}[a.time]-({now:0,today:1,later:2}[b.time]));
  return copy;
}

function card(item){
  const saved=state.saved.has(item.id);
  const joined=state.joined.has(item.id);
  const artClass=item.art==='city'?'':item.art;
  return `<article class="listing" data-id="${item.id}">
    <div class="listing-art ${artClass}" aria-hidden="true"></div>
    <div class="listing-main">
      <div class="tags"><span class="tag">${item.kind}</span><span class="tag">VR / ${item.platform}</span></div>
      <h3>${item.title}</h3>
      <p>${item.description}</p>
      <div class="meta"><span>👥 ${item.current} / ${item.capacity}人</span><span>◷ ${item.when}</span><span>◎ ${item.lang}</span></div>
    </div>
    <div class="listing-side"><button class="save ${saved?'saved':''}" aria-label="お気に入り" data-save="${item.id}">${saved?'♥':'♡'}</button><span class="host">by ${item.host}</span><button class="join ${joined?'joined':''}" data-join="${item.id}">${joined?'参加予定':'参加する'}</button></div>
  </article>`;
}

function render(){
  const visible=ordered(listings.filter(matches));
  container.innerHTML=visible.map(card).join('');
  summary.textContent=`${visible.length}件を表示`;
  empty.hidden=visible.length>0;
  container.hidden=visible.length===0;
  $$('.save').forEach(btn=>btn.addEventListener('click',()=>toggleSaved(btn.dataset.save)));
  $$('.join').forEach(btn=>btn.addEventListener('click',()=>toggleJoined(btn.dataset.join)));
}

function toggleSaved(id){state.saved.has(id)?state.saved.delete(id):state.saved.add(id);render();}
function toggleJoined(id){state.joined.has(id)?state.joined.delete(id):state.joined.add(id);render();}

$('#searchForm').addEventListener('submit',event=>{event.preventDefault();state.query=$('#searchInput').value;render();});
$('#searchInput').addEventListener('input',event=>{state.query=event.target.value;render();});
$$('input[type=checkbox]').forEach(input=>input.addEventListener('change',render));

function wireChipGroup(selector,key,dataKey){
  $$(selector).forEach(button=>button.addEventListener('click',()=>{
    const next=button.dataset[dataKey];
    state[key]=state[key]===next?'':next;
    $$(selector).forEach(el=>el.classList.toggle('selected',el.dataset[dataKey]===state[key]));
    render();
  }));
}
wireChipGroup('#sizeFilters button','size','size');
wireChipGroup('#timeFilters button','time','time');

$('#sortSelect').addEventListener('change',event=>{state.sort=event.target.value;render();});
$$('.tab').forEach(tab=>tab.addEventListener('click',()=>{
  $$('.tab').forEach(t=>t.classList.remove('active'));
  tab.classList.add('active');
  state.tab=tab.dataset.tab;
  if(state.tab==='list'){state.size='';state.time='';syncChips();}
  render();
}));

function syncChips(){
  $$('#sizeFilters button').forEach(el=>el.classList.toggle('selected',el.dataset.size===state.size));
  $$('#timeFilters button').forEach(el=>el.classList.toggle('selected',el.dataset.time===state.time));
}

$('#resetFilters').addEventListener('click',()=>{
  $$('input[type=checkbox]').forEach(input=>input.checked=true);
  state.query='';state.size='medium';state.time='now';state.sort='recommended';
  $('#searchInput').value='';$('#sortSelect').value='recommended';syncChips();render();
});

const createDialog=$('#createDialog');
function openCreate(){createDialog.showModal();}
$('#createButton').addEventListener('click',openCreate);
$('#quickCreate').addEventListener('click',openCreate);
$('#createForm').addEventListener('submit',event=>{
  if(event.submitter?.value==='cancel')return;
  event.preventDefault();
  const capacity=Math.max(2,Math.min(80,Number($('#newCapacity').value)||8));
  listings.unshift({id:`local-${Date.now()}`,title:$('#newTitle').value.trim(),kind:$('#newKind').value,platform:$('#newPlatform').value,current:1,capacity,time:'now',when:'今すぐ',lang:'JP',host:'あなた',description:$('#newDescription').value.trim()||'一緒に遊ぶメンバーを募集中です。',tags:['新着'],art:'city'});
  createDialog.close();
  event.target.reset();
  state.size='';state.time='';state.tab='list';syncChips();
  $$('.tab').forEach(t=>t.classList.toggle('active',t.dataset.tab==='list'));
  render();
});

$('#randomJoin').addEventListener('click',()=>{
  const candidates=listings.filter(matches).filter(item=>!state.joined.has(item.id));
  if(!candidates.length)return;
  const chosen=candidates[Math.floor(Math.random()*candidates.length)];
  state.joined.add(chosen.id);render();
});
$('#showSaved').addEventListener('click',()=>{
  state.tab='saved';$$('.tab').forEach(t=>t.classList.toggle('active',t.dataset.tab==='saved'));render();
});

const guideDialog=$('#guideDialog');
$('#guideButton').addEventListener('click',()=>guideDialog.showModal());
$('#closeGuide').addEventListener('click',()=>guideDialog.close());
$$('.tag-cloud button').forEach(button=>button.addEventListener('click',()=>{
  const value=button.textContent.replace(/^#/,'');
  state.query=value;$('#searchInput').value=value;state.size='';state.time='';syncChips();render();
}));

render();

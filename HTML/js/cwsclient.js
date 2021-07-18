const roomid = document.getElementById('roomid');
const roomname = document.getElementById('roomname');
const mtrip = document.getElementById('mtrkit');

let cwsUrl = window.location.href.replace('Html/', 'cws/settings');

async function getSettings () {
  // let cwsUrl = url.replace('Html/', 'cws/settings')
  let result = await fetch(cwsUrl, {
    headers: new Headers({ 
    'Content-Type': 'application/json'
  })});
  let { data } = await result.json();

  console.log(data);

  roomid.innerText = data.roomid;
  roomname.innerText = data.roomname;
  mtrip.innerHTML = data['mtr-ip'];

  if (data.fileExists == false) {
    document.getElementById('not-ready').innerHTML = data.msg;
  }
}

async function saveSettings() {
  // let cwsUrl = url.replace
  const roomtype = document.getElementById('roomtype');
  const sources = document.getElementById('sources');
  const display = document.getElementById('display');

  let postData = {
    'data': {
      'roomid': roomid.innerText,
      'roomname': roomname.innerText,
      'roomtype': roomtype.value,
      'sources': sources.value,
      'displaytype': display.value
    }
  }
  
  console.log(postData);

  let result = await fetch(cwsUrl, {
    method: 'PUT',
    headers: new Headers({ 'Content-Type': 'application/json' })
  });

  let { data } = result;
  console.log(data);
  if (data.error) {
    alert(`Error posting config file: ${data.error}`);
  } else {
    alert('Configuration has been successfully posted!');
    getSettings();
  }
  
}

setTimeout(getSettings, 100);

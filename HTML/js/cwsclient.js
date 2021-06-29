const roomid = document.getElementById('roomid');
const roomname = document.getElementById('roomname');

let url = window.location.href;

async function getSettings () {
  let cwsUrl = url.replace('Html/', 'cws/settings')
  let result = await fetch(cwsUrl, {
    headers: new Headers({ 
    'Content-Type': 'application/json'
  })});
  let { data } = await result.json();

  console.log(data);

  roomid.innerText = data.roomid;
  roomname.innerText = data.roomname;
}

setTimeout(getSettings, 100);

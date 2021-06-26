const roomid = document.getElementById('roomid');
const roomname = document.getElementById('roomname');

async function getSettings () {
  let result = await fetch('cws/settings', {
    headers: new Headers({ 
    'Content-Type': 'application/json'
  })});
  let { data } = await result.json();

  console.log(data);

  roomid.innerText = data.roomid;
  roomname.innerText = data.roomname;
}

setTimeout(getSettings, 100);

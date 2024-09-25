// discordBot.js
const fs = require('fs');
const path = require('path');
// User.json 파일 경로
const userFilePath = path.join(__dirname, 'User.json');
const { Client, Collection, GatewayIntentBits, Partials } = require('discord.js');
const { joinVoiceChannel, createAudioPlayer, createAudioResource, StreamType, AudioPlayerStatus } = require('@discordjs/voice');
require('dotenv').config();
const client = new Client({
  intents: [
    GatewayIntentBits.DirectMessages,
    GatewayIntentBits.Guilds,
    GatewayIntentBits.GuildMessages,
    GatewayIntentBits.MessageContent,
    GatewayIntentBits.GuildVoiceStates
  ],
  partials: [Partials.Channel],
});
client.commands = new Collection();

// User.json 파일에서 데이터를 읽어 name에 해당하는 id를 반환하는 함수
function getIdByName(name) {
  return new Promise((resolve, reject) => {
    fs.readFile(userFilePath, 'utf8', (err, data) => {
      if (err) {
        if (err.code === 'ENOENT') {
          // 파일이 존재하지 않는 경우
          console.error('User.json 파일이 존재하지 않습니다.');
          return reject('User.json 파일이 존재하지 않습니다.');
        } else {
          console.error('파일을 읽는 중 오류 발생:', err);
          return reject(err);
        }
      }

      let users;
      try {
        users = JSON.parse(data);
      } catch (parseErr) {
        console.error('JSON 파싱 중 오류 발생:', parseErr);
        return reject(parseErr);
      }

      const id = users[name];
      if (id) {
        resolve(id);
      } else {
        reject(`사용자 ${name}을(를) 찾을 수 없습니다.`);
      }
    });
  });
}

// User.json 파일에서 데이터를 읽고, 새로운 데이터를 추가하는 함수
function addUserToFile(name, id) {
  fs.readFile(userFilePath, 'utf8', (err, data) => {
    if (err) {
      if (err.code === 'ENOENT') {
        // 파일이 존재하지 않는 경우, 빈 객체로 초기화
        console.log('User.json 파일이 존재하지 않아 새로 생성합니다.');
        data = '{}';
      } else {
        console.error('파일을 읽는 중 오류 발생:', err);
        return;
      }
    }

    let users;
    try {
      users = JSON.parse(data);
    } catch (parseErr) {
      console.error('JSON 파싱 중 오류 발생:', parseErr);
      users = {};
    }

    // 새로운 사용자 추가
    users[name] = id;

    // JSON 데이터를 문자열로 변환하여 파일에 저장
    fs.writeFile(userFilePath, JSON.stringify(users, null, 2), 'utf8', (writeErr) => {
      if (writeErr) {
        console.error('파일을 쓰는 중 오류 발생:', writeErr);
      } else {
        console.log('사용자 추가 완료:', { name, id });
      }
    });
  });
}

const getKoreanTime = () => {
  const now = new Date();
  const options = {
    timeZone: 'Asia/Seoul',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  };
  const formatter = new Intl.DateTimeFormat('ko-KR', options);
  return formatter.format(now);
};

console.log(getKoreanTime()); // 현재 한국 시간 출력

// 준비
client.on('ready', () => console.log(`디스코드봇 작동 시작!!`));

// 이름등록
client.on('messageCreate', msg => {
  if(msg.author.bot){return};
  if(msg.channel.id !== process.env.NAME_CHANNEL)
    {
      return;
    };

  const name = msg.content;
  const id = msg.author.id;
  addUserToFile(name, id);
  console.log(`사용자 ${name}의 ID: ${id}`);
  const user = msg.guild.members.cache.get(id);
  if (user) {
    msg.channel.send(`${user}님의 로드나인 캐릭터 ${name} 등록 완료`);
  }
})


//사망메시지
const send = async (name) => {
  try {
    const id = await getIdByName(name);
    console.log(`사용자 ${name}의 ID: ${id}`);
    const sendMessage = async () => {
      try {
        const channel = await client.channels.fetch(process.env.TEXT_CHANNEL);
        if (channel) {
          const mention = `<@${id}>`; // 유저 ID를 기반으로 멘션 생성
          await channel.send(`${mention}님의 캐릭터가 사망하였습니다. (${getKoreanTime()})`);
        } else {
          console.log('텍스트 채널을 찾을 수 없습니다.');
        }
      } catch (error) {
        console.error('오류 발생:', error);
      }
    };

    if (client.isReady()) {
      await sendMessage();
    } else {
      client.once('ready', sendMessage);
    }
  } catch (error) {
    console.error('오류 발생:', error);
  }
};

client.login(process.env.DISCORD_KEY);
module.exports = {
  client,
  send
};

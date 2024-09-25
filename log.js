const fs = require('fs');

function getCurrentKoreanTime() {
  const options = { timeZone: 'Asia/Seoul', year: 'numeric', month: 'numeric', day: 'numeric', hour: 'numeric', minute: 'numeric', second: 'numeric', hour12: false };
  const formatter = new Intl.DateTimeFormat('ko-KR', options);
  const now = new Date();
  const formattedDate = formatter.format(now);
  const [year, month, day, hour, minute, second] = formattedDate.match(/\d+/g);
  const formattedDateTime = `${year}년${month}월${day}일 ${hour}시${minute}분${second}초`;
  return formattedDateTime;
}

function log(content)
{
  const date = getCurrentKoreanTime();
  fs.writeFile('./log.txt', date+" "+content+"\n", { flag: 'a+' }, err => {
    if (err) {
      console.error("로그 추가 에러",err)
      return
    }
  })
}

module.exports = {
    log
};
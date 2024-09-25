const express = require("express");
const app = express();
const bodyParser = require("body-parser");
const { client, send } = require("./discordBot.js");
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
const { log } = require("./log.js");

app.listen(9999, () => {
  console.log("Server is running on port 9999");
});

app.post("/", async (req, res) => {
  try {
    if (req.body.name === undefined) {
      res.send("error");
      return;
    }
    log(req.body.name);
    console.log(req.body);
    await send(req.body.name);
    res.send("ok");
  } catch (e) {
    res.send("error");
    return;
  }
});

# Discord-Bot
Before running the bot, create a `credentials.json` file with the following content:

```
credentials.json:
{
  "discord-bot-token": "...",
  "discord-guild-id": "...",
  "openAI-api-key": "...",
  "cloudflare-api-key": "...",
  "cloudflare-account-id": "...",
  "cloudflare-namespace-events": "...",
  "cloudflare-namespace-schedule": "..."
}
```

Commands: (Processing...)

```
e group:
  sked group:
    put
    rm
    ls
    confirm
  arch group:
    put
    rm
    ls
l group:
  put
  rm
  ls
```

DB Tables: (Processing...)

```
DB  Lamp:
    T Event
    T SkedEvent
    T Lead
    T User
    T Status
    T Role
    T EventType
    T EventLead
    T EventUser
```

![Lamp Database](screenshots/00-BD.png)
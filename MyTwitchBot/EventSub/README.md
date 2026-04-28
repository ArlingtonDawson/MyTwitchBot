# Scroll Customization

- `scroll.css` — edit colors, fonts, sizing. The Twitch purple (#9147ff) 
   can be swapped for your own brand color throughout.
- `scroll.html` — the base HTML structure.
- `scroll.js` — controls section order and scroll speed. 
   Adjust `Math.min(30 + (totalEntries * 2), 180)` to change scroll duration.

Data is generated automatically by the bot into `endstream.json` 
when a mod types `!endstream` in chat.
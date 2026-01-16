# WindowsGSM.Hytale
🧩WindowsGSM plugin that provides Hytale Dedicated server

## PLEASE ⭐STAR⭐ THE REPO IF YOU LIKE IT! THANKS!

### Notes, kinda experimental
- Both the Downloader and the game will need a manual authentification on the first start. 
  - The Downloader should print the authCode in the install-Log, authorize it with the link printed There!
- Server auth:
   - Start server and you need click Toggle Console and in console you shoud see WARN "No server tokens configured.
   - You need to type "/auth login device" and you shoud see this same as in step 1. you need to once again authenticate server. Do this and now you got Authentication Succesfull and another WARNING "Credentials stored in memory only - they will be lost on restart!"
   - Now you need to change this to Encrypted by typing: "/auth persistence Encrypted"
   - Now you see green info Credential Storage changed to Encrypted
- !!! The downloader credentials can expire sometime in the future. if your update is stuck for way longer than usual:
  - click Browse => Serverfiles, go to the installer folder and double click the hytale-downloader-windows-amd64.exe and redo the authentification prompted
- Change Javaparameters by clicking Edit config => Server start Param.
- The Assets are set by Server Map (MUSST be the Path from Serverfiles onwards if you create folders)

### Skip Oauth
- To skip the Authentification if you have issues:
  - place the hytale zip file that should be downloaded by the installer into the windowsGSM root
  - The the zip MUSST CONTAIN directly the Assets.zip and the server folder 
  - Other structures WILL NOT WORK.
- Updating works the same way, just place it into the Serverfiles in the Install folder. WGSM Should grab it at the next Startup and delete it afterwards.
  - Click Browse  => Serverfiles, go into the Install folder.
  - place the Hytale.zip there
- Creating the zip yourself
  - Go to following location in your explorer: %appdata%\Hytale\install\release\package\game\latest
  - Zip the "Server"-folder and Assets.zip into a Hytale.zip

### ToDo
- adjust to steam when released

### WindowsGSM Installation: 
1. Download  WindowsGSM https://windowsgsm.com/ 
2. Create a Folder at a Location you wan't all Server to be Installed and Run.
3. Drag WindowsGSM.Exe into previously created folder and execute it.

### Plugin Installation:
1. Download latest release by clicking the green Code Button => Download .zip
2. Either Extract then Move the folder **Hytale.cs** to **WindowsGSM/plugins** 
    1. Press on the Puzzle Icon in the left bottom side and press **[RELOAD PLUGINS]** or restart WindowsGSM
3. Or Press on the Puzzle Icon in the left bottom side and press **[IMPORT PLUGIN]** and choose the downloaded .zip

### Official Documentation
🗃️ https://hytale.game/en/create-server-hytale-guide/

### The Game
🕹️ https://store.steampowered.com/app/16900/GROUND_BRANCH/

### Dedicated server info
🖥️ https://steamdb.info/app/476400/info/

### Port Forwarding (YOU NEED THIS, TO BE ABLE TO CONNECT FROM THE INTERNET(only for servers/pcs at home):
- If You don't know How: portforward.com
- 5520 UDP - Default Game Port

### Files To Backup
- Save Gane (You could only save serverfiles/Hytale/Saved , but that includes many big logs)
- WindowsGSM Config
  - WindowsGSM\servers\%ID%\configs

### Available Params


### Not having an full IPv4 adress ( named CCNAT or DSL Light )
No game or gameserver supports ipv6 only connections. 
- You need to either buy one (most VPN services provide that option. A pal uses ovpn.net for his server, I know of nordvpn also providing that. Should both cost around 7€ cheaper half of it, if your already having an VPN)
- Or you pay a bit more for your internet and take a contract with full ipv4. (depending on your country)
- There are also tunneling methods, which require acces to a server with a full ipv4. Some small VPS can be obtained, not powerfull enough for the servers themself, but only for forwarding. I think there are some for under 5€), the connection is then done via wireguard. but its a bit configuration heavy to setup) 

Or you connect your friends via VPN to your net and play via local lan then.
Many windowsgsm plugin creators recommend zerotier (should be a free VPN designated for gaming) , see chapter below (or tailscale, but no howto there)

## How can you play with your friends without port forwarding?
- Use [zerotier](https://www.zerotier.com/) folow the basic guide and create network
- Download the client app and join to your network
- Create static IP address for your host machine
- Edit WGSM IP Address to your recently created static IP address
- Give your network ID to your friends
- After they've joined to your network
- They can connect using the IP you've created eg: 10.123.17.1:7777
- Enjoy

### Support
[WGSM](https://discord.com/channels/590590698907107340/645730252672335893)

### Give Love!
[Buy me a coffee](https://ko-fi.com/raziel7893)

[Paypal](https://paypal.me/raziel7893)

### License
This project is licensed under the MIT License - see the <a href="https://github.com/raziel7893/WindowsGSM.Hytale/blob/main/LICENSE">LICENSE.md</a> file for details

### Thanks
Thanks to ohmcodes for the Enshrouded and Palworld Plugins which i used for guidance to create this one

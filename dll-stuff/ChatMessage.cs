using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace JabeDll {
    public class ChatMessage
    {
        // OPTIONS
        private static string pathVIEWER = @"D:\Stream\Data\Viewers\";

        private static string user, userID, message, source;
        private static bool isModerator = false; 

        public static void Handle(Dictionary<string, object> args) {
            user = args["user"].ToString();
            userID = args["userId"].ToString();
            message = args["message"].ToString();
            source = args["platform"].ToString();

            if (message.StartsWith("!")) {
                string[] arguments = message.Split(' ');
                string command = arguments[0].Replace("!","");
                string commandPath = "";
                int millisecondsToAdd = 0;

                #region TXT
                if (File.Exists(Settings.PathTxt + command + ".txt") && command != "mod") {
                    // Is a TXT command
                    commandPath = Settings.PathTxt + command + ".txt";
                }
                #endregion
                #region Random TXT
                else if (Directory.Exists(Settings.PathTxt + command) && command != "mod") {
                    // Is a TXT command but runs a random file in that folder
                    Random r = new Random();
                    string[] cmds = Directory.GetFiles(Settings.PathTxt + command);
                    int cmdToExecute = r.Next(cmds.Length);

                    commandPath = Settings.PathTxt + command + @"\" + cmds[cmdToExecute] + ".txt";
                }
                #endregion
                #region Moderator Only TXT
                // Moderator only commands
                else if (File.Exists(Settings.PathTxt + @"mod\" + command + ".txt") && isModerator) {
                    // Is a TXT command but runs a random file in that folder
                    commandPath = Settings.PathTxt + @"mod\" + command + ".txt";
                }
                #endregion
                if (commandPath != "") {
                    ReadCommand(commandPath, arguments);
                }

                // Then deal with the price and cooldown for the executed command
                int price = GetCommandPrice(command);

                // TODO check cooldowns without CPH
                //if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand")) >= 0 && 
                //    DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand" + command)) >= 0 && 
                //    price <= GetUserPoints(userID)) {
                if (true) {    
                    // Charges the user X amount of points to call a command
                    UpdateUserPoints(userID, -price); 

                    #region SFX
                    string sfx = Settings.PathSfx + command + ".mp3";
                    string gfx = Settings.PathGfx + command + ".mp4";
                    if (File.Exists(sfx)) {
                        // Is a SFX command
                        millisecondsToAdd = GetDuration(sfx);
                        // TODO Find a way to send commands to OBS without CPH
                        //CPH.ObsSetSourceVisibility("Component Overlay Effects", "SFX", false);
                        //CPH.ObsSetMediaSourceFile("Component Overlay Effects", "SFX", sfx);
                        //CPH.ObsSetSourceVisibility("Component Overlay Effects", "SFX", true);
                    }
                    #endregion
                    #region Random SFX 
                    else if (Directory.Exists(Settings.PathSfx + command)) {
                        // Is a SFX command but runs a random file in that folder
                        Random r = new Random();
                        string[] cmds = Directory.GetFiles(Settings.PathSfx + command);
                        int cmdToExecute = r.Next(cmds.Length);
                        millisecondsToAdd = GetDuration(cmds[cmdToExecute]);

                        // TODO find a way to send stuff to OBS without CPH
                        //CPH.ObsSetSourceVisibility("Component Overlay Effects", "SFX", false);
                        //CPH.ObsSetMediaSourceFile("Component Overlay Effects", "SFX", cmds[cmdToExecute]);
                        //CPH.ObsSetSourceVisibility("Component Overlay Effects", "SFX", true);
                    }
                    #endregion
                    #region GFX
                    else if (File.Exists(gfx)) {
                        // Is a GFX command
                        int duration = GetDuration(gfx);

                        // TODO find a way to send stuff to OBS without CPH
                        //CPH.ObsSetBrowserSource("Component Overlay Effects", "GFX", gfx);
                        //CPH.ObsSetSourceVisibility("Component Overlay Effects", "GFX", true);
                        //CPH.Wait(duration);
                        //CPH.ObsSetSourceVisibility("Component Overlay Effects", "GFX", false);
                        millisecondsToAdd = duration;
                    }
                    #endregion

                    // Determines when the next command can be executed
                    // TODO use the DB to check cooldowns? without CPH
                      //CPH.SetGlobalVar("canPlayCommand", DateTime.Now.AddMilliseconds(millisecondsToAdd));
                      //CPH.SetGlobalVar("canPlayCommand" + command, DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)));
                }
            }
        }

        private static void ReadCommand(string cmdFile, string[] arguments) {
            bool hasOutput = true;
            string[] lines = File.ReadAllLines(cmdFile);

            //foreach(string l in lines) {
            for (int l=0; l<lines.Length; l++) {
                string output = lines[l];

                // Managing all possible variables in commands
                #region args - Returns a specific argument
                for(var i = 1; i < arguments.Length; i ++) {
                    if (output.Contains("{" + i.ToString() + "}")) {
                        output = output.Replace("{" + i.ToString() + "}", arguments[i]);
                    }
                }
                #endregion
                #region rom - Returns the rest of the message
                if (output.Contains("{rom}")) {
                    string rom = "";
                    for(var i = 1; i < arguments.Length; i ++) {
                        rom += arguments[i] + " ";
                    }
                    output = output.Replace("{rom}", rom);
                }
                #endregion
                #region sender - Returns the user name
                if (output.Contains("{sender}")) {
                    output = output.Replace("{sender}", user);
                }
                #endregion
                #region noOutput - Removes any output the command has
                if (output.Contains("{noOutput}")) {
                    hasOutput = false;
                }
                #endregion
                #region w - Wait for X milliseconds
                if (output.Contains("{w}")) {
                    Data.Log("WAIT");
                    int t = Int32.Parse(output.Replace("{w}", ""));
                    Data.Log(t.ToString());
                    Thread.Sleep(t);
                    output = "";
                }
                #endregion
                #region r - Random from 1 to X
                if (output.Contains("{r}")) {
                    int randomMax = Int32.Parse(output.Replace("{r}", ""));
                    Random r = new Random();
                    output = r.Next(1, randomMax).ToString();
                }
                #endregion
                #region points - Reads and returns user points
                if (output.Contains("{points}")) {
                    output = output.Replace("{points}", GetUserPoints(userID).ToString());
                }
                #endregion
                #region first - The FIRST ONE
                if (output.Contains("{first}")) {
                    // TODO redo without using CPH
                    /*
                    if (CPH.GetGlobalVar<string>("first") == null) {
                        CPH.SetGlobalVar("first", user);
                        CPH.AddToCredits("first", user, false);
                        UpdateUserPoints(userID, 15);
                        output = output.Substring(0, output.IndexOf("{first}"));
                    }
                    else {
                        output = output.Substring(output.IndexOf("{first}") + 7);
                    }
                    */
                }
                #endregion
                #region note - Let's viewers save notes for the streamer
                if (output.Contains("{note}")) {
                    string txt = "";

                    for(var i = 1; i < arguments.Length; i ++) {
                        txt += arguments[i] + " ";
                    }

                    using (StreamWriter writer = new StreamWriter(Settings.PathData + "notes.txt", true))
                        writer.WriteLine(user + ": " + txt);
                }
                #endregion
                #region run - Execute a file
                    if (output.Contains("{run}")) {
                        string file = output.Replace("{run}", "");
                        //TODO Finish the run function
                    }
                #endregion
                #region massfart - MOD - Farts a bunch of time depending on the amount of viewers
                if (output.Contains("{massfart}")) {
                    int amount = Int32.Parse(File.ReadAllLines(Settings.PathData + "viewerCount.txt")[0]) * 2;

                    for(var i=0; i<amount; i++)
                    {
                        // Is a SFX command but runs a random file in that folder
                        Random r = new Random();
                        string[] fartSounds = Directory.GetFiles(Settings.PathSfx + "fart");
                        int cmdToExecute = r.Next(fartSounds.Length);
                        // TODO redo without CPH
                        /*
                        CPH.LogDebug(fartSounds[cmdToExecute]);
                        CPH.PlaySound(fartSounds[cmdToExecute]);

                        CPH.Wait(CPH.Between(250, 1000));
                        */
                    }
                }
                #endregion
                #region resetCD - MOD - Resets the cooldown of a specific command
                if (output.Contains("{resetcd}")) {
                    if (arguments.Length > 0) {
                        // TODO redo without CPH
                        //CPH.SetGlobalVar("canPlayCommand" + arguments[1], DateTime.Now);
                    }
                }
                #endregion
                #region addcmd - MOD - Add a command... FROM A COMMAND
                // TODO MOD adding commands
                #endregion
                #region rmcmd - MOD - Remove a command... FROM A COMMAND
                // TODO MOD remove commands
                #endregion
                #region edtcmd - MOD - Edit a command... FROM A COMMAND
                // TODO MOD editing commands
                #endregion

                // Lists stuff
                #region cmds - Lists all available text commands
                if (output.Contains(@"{cmds}")) {
                    output = output.Replace(@"{cmds}", " ");
                    string[] cmds = Directory.GetFiles(Settings.PathTxt, "*.txt");
                    foreach(string c in cmds) {
                        int i = c.Split('\\').Length;
                        output += c.Split('\\')[i-1].Replace(".txt"," ");
                    }
                }
                #endregion
                #region sfx - Lists all available SFXs
                if (output.Contains("{sfx}")) {
                    output = output.Replace("{sfx}", " ");
                    string[] cmds = Directory.GetFileSystemEntries(Settings.PathSfx);
                    foreach(string c in cmds) {
                        int i = c.Split('\\').Length;
                        output += c.Split('\\')[i-1].Replace(".mp3","") + " ";
                    }
                }
                #endregion
                #region gfx - Lists all available GFXs
                if (output.Contains("{gfx}")) {
                    output = output.Replace("{gfx}", " ");
                    string[] cmds = Directory.GetFiles(Settings.PathGfx);
                    foreach(string c in cmds) {
                        int i = c.Split('\\').Length;
                        output += c.Split('\\')[i-1].Replace(".mp4"," ");
                    }
                }
                #endregion

                // Custom commands asked by viewers
                #region roulette - 6 chances, 1 bullet
                if (output.Contains("{roulette}")) {
                    output = output.Replace("{roulette}", "");

                    // TODO Redo without CPH
                    //int chanceRoulette = CPH.GetGlobalVar<int>("chanceRoulette");
                    int chanceRoulette = 6;

                    //if (CPH.Between(1, chanceRoulette) == 1) {
                    if (new Random.Next(1, 6) == 1) {
                        //chanceRoulette = 6;
                        output = "explose la tronche de " + user + "!!";
                    }
                    else {
                        //chanceRoulette --;
                        output = "tire ?? blanc... il reste " + chanceRoulette.ToString() + " chances...";
                    }

                    //CPH.SetGlobalVar("chanceRoulette", chanceRoulette);
                }
                #endregion

                // Outputs ALL available commands for users to a textfile
                #region outputCommandList - DEBUG - Outputs every commands with cooldowns and prices
                if (output.Contains("{outputTextfile}")) {
                    hasOutput = false;
                    output = "";

                    string[] folders = { Settings.PathTxt, Settings.PathSfx, Settings.PathGfx };
                    foreach(string p in folders) {
                        string[] cmds = Directory.GetFiles(p);
                        output += "#### " + p + " ####\n";
                        foreach(string c in cmds) {
                            int i = c.Split('\\').Length;
                            string cmd = c.Split('\\')[i-1].Split('.')[0];
                            string price = GetCommandPrice(cmd).ToString();
                            string cd = GetCooldown(cmd).ToString();
                            output += cmd + " (cd:" + cd.ToString() + " p:" + price.ToString() + ")\n";
                        }
                    }

                    File.WriteAllText(Settings.PathData + "TEST.txt", output);
                }
                #endregion

                
                if (hasOutput && output != "") {
                    (Settings.UseLumiastream) ? Chatbot.ToLumiastream(source, output) : Chatbot.ToStreamerbot(source, output);
                }
            }
        }

        // Returns the cooldown for a specific command
        private static int GetCooldown(string command) {
            int retVal = 0;

            try {
                string[] cmds = File.ReadAllLines(Settings.PathData + "cooldowns.txt");
                foreach(string l in cmds) {
                    if (l.Contains(command)) {
                        retVal = int.Parse(l.Split('=')[1]);
                        break;
                    }
                }
            }
            catch (Exception e) {
                Data.Log("ERROR: " + command);
                Data.Log(e.ToString());
            }

            return retVal;
        }

        // Returns video duration in milliseconds
        private static int GetDuration(string path) {
            var tfile = TagLib.File.Create(path);
            TimeSpan duration = tfile.Properties.Duration;
            return duration.Milliseconds + (duration.Seconds * 1000);
        }

        // Reads the price for a certain command
        private static int GetCommandPrice(string command) {
            int retVal = 0;

            try {
                string[] cmds = File.ReadAllLines(Settings.PathData + "prices.txt");
                foreach(string l in cmds) {
                    if (l.Contains(command)) {
                        retVal = int.Parse(l.Split('=')[1]);
                        break;
                    }
                }
            }
            catch (Exception e) {
                Data.Log("ERROR: " + command);
                Data.Log(e.ToString());
            }

            return retVal;
        }

        // Returns the amount of points the user have
        private static int GetUserPoints(string uid) {
            string file = pathVIEWER + uid + ".txt";
            int retVal = 0;
            if (File.Exists(file)) {
                string value = File.ReadAllLines(file)[0];
                retVal = int.Parse(value);
            }
            return retVal;
        }

        // Updates the amount of points a user have
        private static void UpdateUserPoints(string uid, int pts) {
            string file = pathVIEWER + uid + ".txt";

            // If the user exists, read his points and update
            int points = (File.Exists(file)) ? Int32.Parse(File.ReadAllLines(file)[0]) + pts : pts;
            
            // And saves the file again
            using (StreamWriter writer = new StreamWriter(file)) {
                writer.WriteLine(points.ToString());
            }
        }
    }
}
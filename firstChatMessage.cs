using System;
using System.IO;


public class CPHInline
{
	public bool Execute()
	{
		// your main code goes here
		string user = args["userId"].ToString();
		string userName = args["user"].ToString();
		string file = @"D:\Stream\Alertes\Viewers\" + user + ".mp3";

		if (File.Exists(file)) {
			CPH.PlaySound(file, 0.5F, true);
		}
		
		CPH.AddToCredits("viewers", userName, false);

		return true;
	}
}
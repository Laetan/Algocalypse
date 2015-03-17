using UnityEngine;
using System.Collections;
using System.IO;

public class FileWrite : MonoBehaviour {
	
	private StreamWriter writer;

	public void write(int evolution, Vector2 score){
		if (writer == null) {
			string filename;
			int i = 0;
			while(true){
				filename = "scoreFile_"+i+".csv";
				if(!File.Exists(filename))
					break;
				i++;
			}
			Debug.Log(filename);
			writer = new StreamWriter(filename);
		}
		writer.WriteLine ("{0};{1}",score [0].ToString().Replace(".",","), score [1].ToString().Replace(".",","));
	}

	void OnApplicationQuit() {
		writer.Close ();
	}

}

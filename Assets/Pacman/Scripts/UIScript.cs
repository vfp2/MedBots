using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour {

	public int high, score, highlevel;

	public List<Image> lives = new List<Image>(3);

	Text txt_score, txt_level, txt_high, txt_highlevel;
	
	// Use this for initialization
	void Start () 
	{
		txt_score = GetComponentsInChildren<Text>()[0];
        txt_level = GetComponentsInChildren<Text>()[1];
		txt_high = GetComponentsInChildren<Text>()[2];
		txt_highlevel = GetComponentsInChildren<Text>()[3];

		for (int i = 0; i < 3 - PacmanGameManager.lives; i++)
	    {
	        Destroy(lives[lives.Count-1]);
            lives.RemoveAt(lives.Count-1);
	    }
	}
	
	// Update is called once per frame
	void Update () 
	{

        high = GameObject.Find("Game Manager").GetComponent<ScoreManager>().High();
		highlevel = GameObject.Find("Game Manager").GetComponent<ScoreManager>().HighLevel();

		// update score text
		score = PacmanGameManager.score;
	    txt_level.text = "Level\n" + (PacmanGameManager.Level + 1);
		txt_score.text = "Score\n" + score;
		txt_high.text = "High Score\n" + high;
		if (txt_highlevel != null) txt_highlevel.text = "High Level\n" + (highlevel + 1);
	}


}

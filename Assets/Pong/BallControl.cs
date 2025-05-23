﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallControl : MonoBehaviour {

	private Rigidbody2D rb2d;
	public int ForceX = 20;
	public int ForceY = 15;

	void GoBall() {
		float rand = Random.Range (0, 2);
		if (rand < 1) {
			rb2d.AddForce (new Vector2 (ForceX, -ForceY));
		} else {
			rb2d.AddForce (new Vector2 (-ForceX, -ForceY));
		}
	}

	// Use this for initialization
	void Start () {
		rb2d = GetComponent<Rigidbody2D> ();
		Invoke ("GoBall", 2);
	}

	void ResetBall() {
		rb2d.linearVelocity = new Vector2 (0, 0);
		transform.position = Vector2.zero;
	}

	void RestartGame() {
		ResetBall ();
		Invoke ("GoBall", 1);
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.collider.CompareTag ("Player")) {
			Vector2 vel;
			vel.x = rb2d.linearVelocity.x;
			vel.y = (rb2d.linearVelocity.y / 2.0f) + (coll.collider.attachedRigidbody.linearVelocity.y / 3.0f);
			rb2d.linearVelocity = vel;
		}
	}

}

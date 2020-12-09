using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveArmy : MonoBehaviour {

	private float movementX = 3f;
	private float movementY = 3f;
	private Vector3 startPos = new Vector3(-11f, -3.5f, -10f);
	private Rigidbody2D _body;
	private BoxCollider2D _box;
	// Use this for initialization
	void Start () {
		_body = GetComponent<Rigidbody2D>();
		_box = GetComponent<BoxCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {
		float deltaX = Input.GetAxis("Horizontal") * movementX;
		float deltaY = Input.GetAxis("Vertical") * movementY;
		Vector2 movement = new Vector2(deltaX, deltaY);
		_body.velocity = movement;
		
		// Debug.Log(village_collision);
		// if (village_collision != null){
			
		// 	Debug.Log("we have won");
		// }
		// // Debug.Log("We are moving to " + startPos);dw
		
	}


	// void OnCollisionEnter2D(Collision2D col) {
	// 	if (col.collider.name =="Mountain"){
	// 		Debug.Log("You hit the mountain");
	// 	} else if (col.collider.name == "Village"){
	// 		Debug.Log("You hit the village");
	// 	}
	// }
}

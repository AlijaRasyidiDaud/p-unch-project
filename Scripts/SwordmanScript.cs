using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordmanScript : MonoBehaviour {

	private Rigidbody2D rb;
	private Animator anim;
	private float inputX;
	private bool isGround;
	private float lastAttackTime;
	public float jumpForce;
	public float speed;
	public int health;
	private int maxHealth; // Maksimum health
	private float healthScale; // Skala untuk mengontrol health bar
	public Transform meleePoint; // Posisi damage point
	public float meleeRange; // Range damage point
	public int meleeDamage; // Damage yang diberi
	public float attackDelay;
	public float hitDelay;
	private bool isHurt; // Cek apakah terkena hit
	private bool isDeath; // Cek apakah mati
	public Transform healthBar; // UI health bar
	public Transform healthBarUI; // UI health bar
	public Transform healthBarHolder; // Setter posisi health bar


	// Use this for initialization
	void Start () {
		
		rb = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		lastAttackTime = -1f; // Kalibrasi delay attack (bug pada awal game)

		// Inisiasi isHurt dan isDeath
		isHurt = false;
		isDeath = false;

		// Inisialisasi heath bar
		maxHealth = health;
		healthBar.localScale = new Vector3 (1f, 1f, 1f);
		healthBarUI.position = healthBarHolder.position; // Set posisi health bar

	}
	
	// Update is called once per frame
	void Update () {
		healthBarUI.position = healthBarHolder.position; // Set posisi health bar
		
		// Pembeda saat terkena hurt dan tidak, agar hierarki hurt diutamakan
		if (!isHurt) // Kondisi tak terkena serangan
		{
			// Animasi idle
			anim.SetBool ("Ground", isGround);

			// Ambil input horizontal
			inputX = Input.GetAxis ("Horizontal");

			// Mengganti arah
			if (inputX > 0f)
			{
				transform.localScale = new Vector3 (-1f, 1f, 1f);
			} else if (inputX < 0f)
			{
				transform.localScale = new Vector3 (1f, 1f, 1f);
			}

			// Cek apakah di tanah
			if (isGround)
			{
				// Move
				rb.velocity = new Vector2 (inputX * speed * Time.deltaTime, 0f);

				// Animasi walk
				anim.SetFloat ("Speed", Mathf.Abs (inputX));

				// Jump
				if (Input.GetButtonDown ("Jump"))
				{
					FindObjectOfType <AudioManager> ().Play ("Jump"); // Memanggil sfx jump
					rb.AddForce (Vector2.up * jumpForce);
					//isGround = false;
					anim.SetBool ("Ground", !isGround);
				}

				// Attack
				if (Input.GetMouseButtonDown (0) && (Time.time > lastAttackTime + attackDelay))
				{
					anim.Play ("Attack");

					// Kalibrasi agar animasi hit enemy sinkron dgn damage attack
					Invoke ("HitEnemy", hitDelay);

					lastAttackTime = Time.time; // Set agar delay 
				}


			} else // Not isGround / di udara
			{
				
			}
		} else // Jika terkena hit musuh
		{
			if (!isDeath) // Jika belum mati
			{
				anim.Play ("Hurt");
				Invoke ("DelayHit", 0.5f); // Mendelay kondisi hurt agar ada recovery time
			} else // Jika sudah mati
			{
				anim.Play ("Mati");
			}
		}

	}

	// Fungsi TakeDamage yang dipanggil dari luar (saat terkena serangan)
	void TakeDamage (int damage)
	{
		FindObjectOfType <AudioManager> ().Play ("Player Hurt"); // Memanggil sfx hurt
		isHurt = true; // Mengirim sinyal bahwa terkena hit dari player
		health -= damage;
		healthScale = (float) health / (float) maxHealth; // Menset persenan healt tersisa
		healthBar.localScale = new Vector3 (healthScale, 1f, 1f); // Menset tampilan health bar
		if (health <= 0)
		{
			health = 0;
			isDeath = true; // Mengirim sinyal bahwa health mencapai 0
			Destroy (healthBarUI.gameObject, 1.5f); // Destroy health bar
			Destroy (gameObject, 1.5f); // Delay agar menampilkan animasi death
		}
	}

	// Delaying hurt animation
	void DelayHit ()
	{
		isHurt = false;
	}

	// Cek apakah player di tanah / di ground
	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.CompareTag ("Ground") || other.CompareTag ("Block"))
		{
			isGround = true;
		}

		// Jika player stuck di kepala enemy
		if (other.CompareTag ("Bandit"))
		{
			rb.AddForce (new Vector2 (0f, 200f));
		}
	}

	// Cek apakah player stay di collider (penambahan dibutuhkan pada block tag)
	void OnTriggerStay2D(Collider2D other)
	{
		if (other.CompareTag ("Ground") || other.CompareTag ("Block"))
		{
			isGround = true;
		}

		// Jika player stuck di kepala enemy
		if (other.CompareTag ("Bandit"))
		{
			rb.AddForce (new Vector2 (0f, 200f));
		}
	}

	// Mengecek ketika player sesaat keluar dari trigger collider
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag ("Ground") || other.CompareTag ("Block"))
		{
			isGround = false;
		}
	}

	// Fungsi memberikan perintah damaging ke target
	void HitEnemy ()
	{
		// Cek apakah animasi attack masih berlangsung
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
		{
			FindObjectOfType <AudioManager> ().Play ("Attack"); // Menjalankan sfx attack
			Collider2D[] hitObjects = Physics2D.OverlapCircleAll (meleePoint.position, meleeRange);
			for (int i = 0; i < hitObjects.Length; i++)
			{
				if (hitObjects[i].CompareTag ("Bandit"))
				{
					hitObjects[i].SendMessage ("TakeDamage", meleeDamage, SendMessageOptions.DontRequireReceiver);
					Debug.Log ("Hit");
				}
			}
		}
	}

}

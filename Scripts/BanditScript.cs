using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditScript : MonoBehaviour {

	public int health; // Jumlah health enemy
	private int maxHealth; // Maksimum health
	private float healthScale; // Skala untuk mengontrol health bar
	public Transform[] patrolPoints; // Array posisi patrol
	public float speed; // Kecepatan gerak AI
	public Transform target; // Target player
	public float awarenessRange; // Range deteksi AI
	public float attackRange; // Attack range dari AI
	private float distanceToTarget; // Jarak ke player
	private bool isHurt; // Cek bila terkena hit
	private bool isDeath; // Cek jika mati
	public float hitDelay; // Kalibrasi animasi attack dengan damage mekanism
	private float lastAttackTime; // Mekanisme delay
	public float attackDelay; // Delay attack per attack
	public Transform meleePoint; // Posisi damage point
	public float meleeRange; // Range damage point
	public int meleeDamage; // Damage yang diberi

	Transform currentPatrolPoint; // Posisi patrol sekarang
	int currentPatrolIndex; // Indeks array posisi patrol sekarang

	private Animator anim;
	private Rigidbody2D rb;
	public Transform healthBar; // Health bar scale
	public Transform healthBarUI; // UI health bar
	public Transform healthBarHolder; // Setter posisi health bar

	// Use this for initialization
	void Start () {
		
		anim = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody2D> ();

		// Inisialisasi heath bar
		maxHealth = health;
		healthBar.localScale = new Vector3 (1f, 1f, 1f);
		healthBarUI.position = healthBarHolder.position; // Set posisi health bar

		// Inisiasi patrol point
		currentPatrolIndex = 0;
		currentPatrolPoint = patrolPoints[currentPatrolIndex];

		// Inisialisasi kondisi hurt dan death
		isHurt = false;
		isDeath = false;

	}
	
	// Update is called once per frame
	void Update () {
		healthBarUI.position = healthBarHolder.position; // Set posisi health bar

		// Ukur jarak ke player
		distanceToTarget = Vector3.Distance (transform.position, target.position);

		// Cek apakah terkena hit player (dibutuhkan agar menjalankan animasi sesuai hierarki)
		if (!isHurt) // Kondisi tak terkena hit dari player
		{
			// Kondisi jika player memasuki batas jarak deteksi AI
			if (distanceToTarget < awarenessRange && distanceToTarget > attackRange)
			{
				if (target.position.y - transform.position.y < 2f)
				{
					Chase ();
				} else
				{
					anim.Play ("LightGuard_Idle");
				}
			} 
			
			// Kondisi jika player masuk batas deteksi serang AI (attack)
			if (distanceToTarget < attackRange && Time.time > lastAttackTime + attackDelay)
			{
				anim.Play ("HeavyBandit_Attack");

				// Kalibrasi agar animasi hit enemy sinkron dgn damage attack
				Invoke ("Attack", hitDelay);

				lastAttackTime = Time.time; // Set delay
			} 

		} else // Terkena hit player
		{
			if (!isDeath)
			{
				anim.Play ("HeavyBandit_Hurt"); // Menjalankan animasi hurt
				Invoke ("DelayHit", 0.5f); // Mendelay kondisi hurt agar ada recovery time
			} else
			{
				anim.Play ("HeavyBandit_Death"); // Menjalankan animasi death
			}
		}

		
	}

	// Fungsi TakeDamage yang dipanggil dari luar
	void TakeDamage (int damage)
	{
		FindObjectOfType <AudioManager> ().Play ("Enemy Hurt");
		isHurt = true; // Mengirim sinyal bahwa terkena hit dari player
		health -= damage;
		healthScale = (float) health / (float) maxHealth; // Menset persenan health tersisa
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

	// Fungsi patrol AI
	void Patrol ()
	{
		// Jalankan animasi
		anim.Play ("HeavyBandit_Run");

		// Looping patrol point
		if (Vector3.Distance(transform.position, currentPatrolPoint.position) < 1f)
		{
			if (currentPatrolIndex + 1 < patrolPoints.Length)
			{
				currentPatrolIndex++;
			} else
			{
				currentPatrolIndex = 0;
			}

			currentPatrolPoint = patrolPoints[currentPatrolIndex];
		}

		// Gerakan dan fungsi hadap
		Vector3 patrolPointDir = currentPatrolPoint.position - transform.position; // Arah vektor menuju patrol point
		Vector3 newScale;
		if (patrolPointDir.x < 0f) // Hadap kiri
		{
			rb.velocity = Vector2.left * speed * Time.deltaTime;
			newScale = new Vector3(3.37715f, 3.980056f, 1);
			transform.localScale = newScale;
		} else if (patrolPointDir.x > 0f) // Hadap kanan
		{
			rb.velocity = Vector2.right * speed * Time.deltaTime;
			newScale = new Vector3(-3.37715f, 3.980056f, 1);
			transform.localScale = newScale;
		}
	}

	// Fungsi chase AI
	void Chase ()
	{
		// Jalankan animasi
		anim.Play ("HeavyBandit_Run");

		// Gerakan dan fungsi hadap
		Vector3 targetDir = target.position - transform.position; // Arah vektor menuju target player
		Vector3 newScale;
		if (targetDir.x < 0f) // Hadap kiri
		{
			rb.velocity = Vector2.left * speed * Time.deltaTime;
			newScale = new Vector3(3.37715f, 3.980056f, 1);
			transform.localScale = newScale;
		} else if (targetDir.x > 0f) // Hadap kanan
		{
			rb.velocity = Vector2.right * speed * Time.deltaTime;
			newScale = new Vector3(-3.37715f, 3.980056f, 1);
			transform.localScale = newScale;
		}
	}

	// Fungsi Attack AI
	void Attack ()
	{
		// Cek apakah animasi attack masih berlangsung
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyBandit_Attack"))
		{
			FindObjectOfType <AudioManager> ().Play ("Attack");
			Collider2D[] hitObjects = Physics2D.OverlapCircleAll (meleePoint.position, meleeRange);
			for (int i = 0; i < hitObjects.Length; i++)
			{
				if (hitObjects[i].CompareTag ("Player"))
				{
					hitObjects[i].SendMessage ("TakeDamage", meleeDamage, SendMessageOptions.DontRequireReceiver);
					Debug.Log ("Hit");
				}
			}
		}
	}
}

using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public Sound[] sounds; // Class buatan yang berisi sifat dari sound

	public static AudioManager instance; // Referensi diri

	// Awake dipanggil saat object dibuat
	void Awake () {
		// karena objek tidak destroy saat meload scene baru, memungkinkan terdapat 2 objek serupa
		// maka jika ada, salahsatunya di destroy
		if (instance == null)
		{
			instance = this;
		} else
		{
			Destroy (gameObject);
			return;
		}

		// Objek tak destroy saat meload objek baru
		DontDestroyOnLoad (gameObject);

		// Meload sound yang tersimpan (sound di tambahkan/diedit dari inspektor)
		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;

			s.source.volume = s.volume; // Volume sound
			s.source.pitch = s.pitch;
			s.source.loop = s.loop; // Kondisi looping
		}
	}

	// Background music yang di tempelkan pada kelas ini (optional)
	void Start ()
	{
		Play ("Theme"); // Background music dengan nama track "Theme"
	}
	
	// Fungsi play musik / sound
	public void Play (string name)
	{
		Sound s = Array.Find (sounds, sound => sound.name == name);
		s.source.Play ();
	}
}

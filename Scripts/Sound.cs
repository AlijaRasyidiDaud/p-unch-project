using UnityEngine.Audio;
using UnityEngine;

[System.Serializable] // Dibutuhkan agar tampil di inspektor
public class Sound {
// Kelas dibuat untuk menyimpan format dari musik / sound

	public string name; // Menyimpan nama (buatan)

	public AudioClip clip; // Menyimpan clip musik itu sendiri

	[Range (0f, 1f)] // Menampilkan range pada inspektor
	public float volume; // Volume musik / sound
	[Range (.1f, 3f)] // Menampilkan range pada inspektor
	public float pitch; // Pitch musik / sound

	public bool loop; // Apakah musik me loop ?

	[HideInInspector] // Tak tampil di inspektor
	public AudioSource source; 

}

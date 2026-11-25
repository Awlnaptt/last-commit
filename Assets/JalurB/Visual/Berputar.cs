using UnityEngine;

// Memutar objek terus-menerus (kipas angin langit-langit).
// Rotasi = sudut bertambah tiap frame: sudut += kecepatan * waktu per frame.
public class Berputar : MonoBehaviour
{
    public Vector3 sumbu = Vector3.up;
    public float derajatPerDetik = 140f;

    void Update() => transform.Rotate(sumbu, derajatPerDetik * Time.deltaTime, Space.Self);
}

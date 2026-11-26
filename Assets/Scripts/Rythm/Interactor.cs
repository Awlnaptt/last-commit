using UnityEngine;

public class Interactor : MonoBehaviour
{
    public Camera cam;
    public float distance = 3.5f;

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || !cam.gameObject.activeInHierarchy || UIStatus.MenghalangiPemain)
        {
            HUDUI.SetPetunjuk("");
            return;
        }

        IInteraktif target = null;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            target = hit.collider.GetComponentInParent<IInteraktif>();

        if (target != null && target.BisaDipakai)
        {
            HUDUI.SetPetunjuk(target.Petunjuk);
            if (Input.GetKeyDown(KeyCode.E)) target.Interaksi();
        }
        else
        {
            HUDUI.SetPetunjuk("");
        }
    }
}
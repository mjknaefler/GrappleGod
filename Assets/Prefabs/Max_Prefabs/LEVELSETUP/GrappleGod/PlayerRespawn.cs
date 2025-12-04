using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private CinemachineCamera[] vcams;
    [SerializeField] private bool instantSnap = true;

    public void Respawn(Transform point)
    {
        var rb = GetComponent<Rigidbody2D>();
        Vector3 oldPos = transform.position;
        if (rb) rb.linearVelocity = Vector2.zero;
        transform.position = point.position;

        if (vcams == null || vcams.Length == 0)
            vcams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        Vector3 delta = transform.position - oldPos;
        for (int i = 0; i < vcams.Length; i++)
            if (vcams[i]) vcams[i].OnTargetObjectWarped(transform, delta);

        if (instantSnap) StartCoroutine(RecenterNextFrame());
    }

    IEnumerator RecenterNextFrame()
    {
        if (vcams == null) yield break;

        var pcs = new List<CinemachinePositionComposer>();
        var dampingOrig = new List<Vector3>();
        var centerOrig = new List<bool>();
        var enabledOrig = new List<bool>();

        for (int i = 0; i < vcams.Length; i++)
        {
            var vcam = vcams[i];
            if (!vcam) { enabledOrig.Add(false); continue; }

            var pc = vcam.GetComponent<CinemachinePositionComposer>();
            pcs.Add(pc);
            dampingOrig.Add(pc ? pc.Damping : Vector3.zero);
            centerOrig.Add(pc ? pc.CenterOnActivate : false);

            if (pc)
            {
                pc.Damping = Vector3.zero;      
                pc.CenterOnActivate = true;     
            }

            enabledOrig.Add(vcam.enabled);
            vcam.enabled = false;               
        }

        yield return null;                       

        for (int i = 0, j = 0; i < vcams.Length; i++)
        {
            var vcam = vcams[i];
            if (!vcam) continue;
            vcam.enabled = enabledOrig[i];       

            var pc = pcs[j];
            if (pc)
            {
                pc.Damping = dampingOrig[j];
                pc.CenterOnActivate = centerOrig[j];
            }
            j++;
        }
    }
}

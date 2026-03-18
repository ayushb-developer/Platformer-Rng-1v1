using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] Transform visualRoot;
    [SerializeField] SpriteRenderer playerSprite;
    [SerializeField] TMP_Text nameText;
    [SerializeField] GameObject crown;
    PlayerController player;

    Rigidbody2D rb;
    bool wasGrounded;

    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }
    public void Initialize(Rigidbody2D body, bool isOwner)
    {
        rb = body;
        playerSprite.enabled = true;
        
        if(isOwner)
        {
            playerSprite.color = Color.blue;
        }
        else
        {
            playerSprite.color = Color.orange;
        }
    }

    public void UpdateVisual(bool grounded)
    {
        // if(GameFlow.Instance.State != GameState.Playing) return;
        // if (playerNet == null || !playerNet.IsSpawned) return;
        // if (!player.IsReady) return;//

        nameText.text = player.IsOwner ? "YOU" : player.OwnerClientId.ToString();

        if(GameFlow.Instance.State!= GameState.Playing)
        {
            return;
        }

        crown.SetActive(
            RaceManager.Instance.LeadingPlayer == player
        );
        //speed-based tilt
        float tilt = Mathf.Clamp(rb.linearVelocity.x * -2f, -15f, 15f);
        visualRoot.rotation = Quaternion.Euler(0, 0, tilt);

        //stretch based on vertical speed (jumping/squashing)
        float stretch = Mathf.InverseLerp(-10, 10, rb.linearVelocity.y);
        Vector3 targetScale = new Vector3(
            1 - stretch * 0.2f,
            1 + stretch * 0.2f,
            1
        );
        visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, targetScale, Time.deltaTime * 10f);

        if (!wasGrounded && grounded)
        {
            visualRoot.localScale = new Vector3(1.2f, 0.8f, 1);
        }

        wasGrounded = grounded;
    }

    internal void DisableVisual()
    {
        playerSprite.enabled = false;
    }
}
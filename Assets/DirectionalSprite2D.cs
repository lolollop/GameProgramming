using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DirectionalSprite2D : MonoBehaviour
{
    public Sprite leftSprite;
    public Sprite rightSprite;
    public bool faceRightByDefault = true;
    public bool swapLeftRight;

    private SpriteRenderer spriteRenderer;
    private bool facingRight;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        facingRight = faceRightByDefault;
        ApplyCurrentSprite();
    }

    public void SetFacing(float horizontalDirection)
    {
        if (horizontalDirection > 0.01f)
        {
            facingRight = true;
        }
        else if (horizontalDirection < -0.01f)
        {
            facingRight = false;
        }

        ApplyCurrentSprite();
    }

    private void ApplyCurrentSprite()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Sprite logicalRight = swapLeftRight ? leftSprite : rightSprite;
        Sprite logicalLeft = swapLeftRight ? rightSprite : leftSprite;
        Sprite targetSprite = facingRight ? logicalRight : logicalLeft;
        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }
    }
}

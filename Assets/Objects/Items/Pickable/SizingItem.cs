using UnityEngine;

[ExecuteAlways]
public class SizingItem : PickableItem
{
    [Header("Sizing Settings")]
    public PlayerSize targetSize = PlayerSize.Normal;


    public override void OnPickUp(PlayerItemManager picker)
    {
        base.OnPickUp(picker);

        if (picker.playerController != null)
        {
            picker.playerController.setSize(targetSize);
        }
    }

        public override void OnDrop()

        {
            if (holder != null && holder.playerController != null)
            {
                holder.playerController.setSize(PlayerSize.Normal);
            }
            base.OnDrop();
        }

    #if UNITY_EDITOR
        protected override void Update()

        {
            base.Update();
        }
    #endif
        protected override void OnValidate()

        {
            base.OnValidate();
        }

    }

    
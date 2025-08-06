using UnityEngine;

public class InstantHealItem : MonoBehaviour
{
    [SerializeField] private float _healAmount;


    [SerializeField] private GameObject _healVFX;
    [SerializeField] private Vector2 _offset;


    public void Use(PlayerController player)
    {
        if (player._playerHealth.MaxHealth == player._playerHealth.CurrentHealth)
            return;

        player._playerHealth.Heal(_healAmount);

        var vfx = Instantiate(_healVFX);
        vfx.transform.SetParent(player.transform);
        vfx.transform.localPosition = _offset;
        Destroy(vfx, 1.5f);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController controller))
        {
            Use(controller);
        }
    }


}

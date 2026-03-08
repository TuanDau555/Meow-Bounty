using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TeamHUDManager : MonoBehaviour
{
    public static TeamHUDManager Instance;

    [System.Serializable]
    public class PlayerSlotUI {
        public GameObject root;         // Cả cái ô UI (Panel đen mờ)
        public Text nameText;           // Chỗ hiện tên
        public Slider healthSlider;     // Thanh máu
        public bool isOccupied = false; // Ô này có ai ngồi chưa?
        public ulong clientId;          // ID của người ngồi ô này
    }

    public List<PlayerSlotUI> slots = new List<PlayerSlotUI>();

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);
    }

    // Hàm để người chơi đăng ký vào 1 ô khi vừa vào game
    public void RegisterPlayer(ulong id, string name) {
        // Kiểm tra xem ID này đã được đăng ký chưa để tránh trùng
        foreach (var s in slots) {
            if (s.isOccupied && s.clientId == id) return;
        }

        for (int i = 0; i < slots.Count; i++) {
            if (!slots[i].isOccupied) {
                slots[i].isOccupied = true;
                slots[i].clientId = id;
                slots[i].nameText.text = name;
                slots[i].healthSlider.value = 1f; // Máu đầy khi mới vào
                slots[i].root.SetActive(true);
                Debug.Log($"Đã đăng ký người chơi {name} vào ô số {i}");
                return;
            }
        }
    }

    // Cập nhật máu cho một người chơi cụ thể (SỬA LỖI == Ở ĐÂY)
    public void UpdateTeamHealth(ulong id, float healthPercent) {
        foreach (var slot in slots) {
            // Sửa lỗi: Phải dùng dấu == để so sánh
            if (slot.isOccupied && slot.clientId == id) {
                slot.healthSlider.value = healthPercent;
                break;
            }
        }
    }

    // Hàm giải phóng ô khi người chơi thoát (Để game mượt hơn)
    public void UnregisterPlayer(ulong id) {
        foreach (var slot in slots) {
            if (slot.isOccupied && slot.clientId == id) {
                slot.isOccupied = false;
                slot.root.SetActive(false); // Ẩn ô đó đi
                break;
            }
        }
    }
}
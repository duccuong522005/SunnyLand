# Scenes Directory

## 🎮 Scene chính: MainGame.unity

**MainGame.unity** là scene chính để test đầy đủ game.

### Cách sử dụng:
1. **Khi muốn test đầy đủ game:**
   - Mở `MainGame.unity`
   - Bấm **Play** để chạy game

2. **Khi làm việc:**
   - **KHÔNG** mở MainGame.unity để sửa
   - Mở **Prefab** riêng của mình để sửa (double-click Prefab)
   - Sau khi sửa xong, test trong MainGame.unity

### Quy tắc:
- ✅ Chỉ kéo Prefabs vào scene
- ✅ Test bằng cách bấm Play
- ❌ KHÔNG sửa trực tiếp GameObject trong scene
- ❌ KHÔNG commit scene mỗi lần test

---

## 📁 Các Scene khác

- **SampleScene.unity** - Scene mẫu (có thể dùng để test riêng)
- **Demo_characters.unity** - Demo characters
- **Demo_environment.unity** - Demo environment
- **Demo_tileset.unity** - Demo tileset

---

## 💡 Workflow

1. **Làm việc:** Mở Prefab → Sửa trong Prefab Mode
2. **Test:** Mở MainGame.unity → Bấm Play
3. **Commit:** Chỉ commit Prefab và Scripts, không commit Scene trừ khi cần thiết

Xem `WORKFLOW_TEAM.md` ở thư mục gốc để biết chi tiết.


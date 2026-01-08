# Unity Team Workflow - Tránh Conflict Scene

## 🚨 Vấn đề

Khi nhiều người cùng làm việc trên một file `.unity` (Scene), Git sẽ gặp **conflict nghiêm trọng** vì:
- File `.unity` là file nhị phân phức tạp
- Git không thể merge tự động
- Dễ mất công sức khi resolve conflict

## ✅ Giải pháp: Sử dụng Prefabs

### Nguyên tắc làm việc:

1. **KHÔNG BAO GIỜ** sửa trực tiếp GameObject trong Scene
2. **LUÔN LUÔN** tạo Prefab trước khi thêm vào Scene
3. Mỗi người làm việc trên Prefab riêng của mình

---

## 📁 Cấu trúc Prefabs

```
Assets/Prefabs/
├── Map/              # Người xây Map sử dụng
│   ├── Platforms/
│   ├── Obstacles/
│   └── Backgrounds/
├── Player/           # Người làm tính năng Player
│   ├── Player.prefab
│   └── PlayerVariants/
├── Enemies/          # Người làm tính năng Enemy
│   ├── Bat.prefab
│   ├── Bear.prefab
│   └── ...
├── Items/            # Người làm tính năng Items
│   ├── Cherry.prefab
│   └── Gem.prefab
├── Environment/       # Người xây Map
│   ├── Trees/
│   ├── Houses/
│   └── Props/
└── UI/               # Người làm UI
    ├── HUD.prefab
    └── Menus/
```

---

## 👥 Workflow cho từng vai trò

### 🗺️ Người xây Map (Level Designer)

**Công việc:**
- Tạo các Prefab cho môi trường (platforms, obstacles, backgrounds)
- Đặt Prefab vào Scene
- **KHÔNG** sửa trực tiếp GameObject trong Scene

**Quy trình:**
1. Tạo GameObject trong Scene (tạm thời)
2. Kéo GameObject vào thư mục `Assets/Prefabs/Map/` để tạo Prefab
3. Xóa GameObject tạm trong Scene
4. Kéo Prefab từ Project vào Scene
5. Commit Prefab, **KHÔNG commit Scene** (hoặc commit Scene chỉ khi cần thiết)

**Lưu ý:**
- Mỗi platform/obstacle nên là Prefab riêng
- Đặt tên Prefab rõ ràng: `Platform_Grass_01.prefab`
- Sử dụng Prefab Variant nếu cần biến thể nhỏ

---

### 🎮 Người làm tính năng (Feature Developer)

**Công việc:**
- Tạo Prefab cho Player, Enemies, Items
- Thêm Scripts vào Prefab
- Test tính năng

**Quy trình:**
1. Tạo Prefab trong `Assets/Prefabs/Player/` (hoặc Enemies/Items)
2. Thêm Scripts vào Prefab
3. Test trong Scene riêng hoặc Scene test
4. Commit Prefab và Scripts
5. **KHÔNG** commit Scene chính trừ khi thực sự cần

**Lưu ý:**
- Mỗi tính năng nên có Prefab riêng
- Scripts nên được attach vào Prefab, không phải Scene instance
- Sử dụng Prefab Variant để tạo biến thể

---

## 🔄 Quy trình làm việc hàng ngày

### Buổi sáng (Pull code mới):
```bash
git pull origin feature/player-controller
```
1. Mở Unity, đợi import xong
2. Mở Scene chính
3. Kiểm tra Prefabs mới từ team
4. Nếu có Prefab mới, kéo vào Scene nếu cần

### Trong khi làm việc:
1. **Tạo Prefab mới:**
   - Tạo GameObject trong Scene
   - Kéo vào thư mục Prefabs phù hợp
   - Xóa instance trong Scene
   - Kéo Prefab vào Scene

2. **Sửa Prefab hiện có:**
   - Mở Prefab trong Prefab Mode (double-click Prefab)
   - Sửa trong Prefab Mode
   - Tất cả instances sẽ tự động update

3. **Thêm vào Scene:**
   - Chỉ kéo Prefab vào Scene
   - **KHÔNG** sửa trực tiếp instance trong Scene

### Buổi tối (Commit code):
```bash
git add Assets/Prefabs/
git add Assets/Scripts/
# KHÔNG add Scene trừ khi thực sự cần
git commit -m "feat: add new enemy prefab"
git push origin feature/player-controller
```

---

## ⚠️ Quy tắc vàng

### ✅ NÊN LÀM:
- ✅ Tạo Prefab cho mọi GameObject
- ✅ Sửa Prefab trong Prefab Mode
- ✅ Commit Prefabs thường xuyên
- ✅ Đặt tên Prefab rõ ràng
- ✅ Sử dụng Prefab Variant cho biến thể

### ❌ KHÔNG NÊN:
- ❌ Sửa trực tiếp GameObject trong Scene
- ❌ Commit Scene mỗi lần thay đổi nhỏ
- ❌ Tạo GameObject phức tạp trực tiếp trong Scene
- ❌ Override Prefab properties trong Scene (trừ khi thực sự cần)

---

## 🛠️ Khi gặp Conflict Scene

Nếu vẫn gặp conflict trên file `.unity`:

### Cách 1: Chấp nhận một bên (Recommended)
```bash
# Chấp nhận version của người khác
git checkout --theirs Assets/Scenes/SampleScene.unity

# Hoặc chấp nhận version của mình
git checkout --ours Assets/Scenes/SampleScene.unity
```

Sau đó:
1. Mở Scene trong Unity
2. Kiểm tra Prefabs bị mất
3. Kéo lại Prefabs từ Project vào Scene

### Cách 2: Sử dụng Unity Scene Merge Tool
- Unity có công cụ merge Scene (nhưng phức tạp)
- Khuyến nghị: Tránh conflict bằng cách dùng Prefabs

---

## 📝 Checklist trước khi Commit

- [ ] Đã tạo Prefab cho mọi GameObject mới?
- [ ] Đã test Prefab hoạt động đúng?
- [ ] Đã đặt tên Prefab rõ ràng?
- [ ] Có cần commit Scene không? (Thường là KHÔNG)
- [ ] Đã pull code mới nhất chưa?

---

## 🎯 Best Practices

1. **Mỗi người một Scene riêng để test:**
   - `SampleScene_MapDesigner.unity` - Cho người xây Map
   - `SampleScene_PlayerDev.unity` - Cho người làm Player
   - `SampleScene_Main.unity` - Scene chính (ít commit)

2. **Sử dụng Scene Templates:**
   - Tạo Scene template với setup cơ bản
   - Mỗi người tạo Scene mới từ template

3. **Prefab Variants:**
   - Dùng Variant cho biến thể nhỏ
   - Giữ Prefab gốc, tạo Variant cho customization

4. **Nested Prefabs:**
   - Tạo Prefab con trong Prefab cha
   - Dễ quản lý và tái sử dụng

---

## 📚 Tài liệu tham khảo

- [Unity Prefabs Documentation](https://docs.unity3d.com/Manual/Prefabs.html)
- [Unity Prefab Variants](https://docs.unity3d.com/Manual/PrefabVariants.html)
- [Unity Collaboration Best Practices](https://docs.unity3d.com/Manual/UnityCollaborate.html)

---

**Nhớ:** Prefabs là bạn, Scene là kẻ thù khi làm việc nhóm! 🎯


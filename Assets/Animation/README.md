Tổng hợp các bước để set up animation clip trong Unity:

1. **Chuẩn bị Animation Clip**:
- Tìm Animation clip trong Project window (ví dụ: PlayerWalking.anim, PlayerStanding.anim)
- Click vào Animation clip để xem Inspector

2. **Thiết lập trong Inspector của Animation Clip**:
```
- Loop Time: ✓ (tích chọn để animation lặp lại)
- Loop Pose: ✓ (tích chọn để animation mượt mà khi lặp)
- Cycle Offset: 0
- FPS: 60 (có thể điều chỉnh tốc độ animation)
```

3. **Thiết lập trong Animator window**:
- Chọn state (ví dụ: PlayerWalking)
- Trong Inspector, phần Motion:
  - Kéo file .anim tương ứng vào ô trống

4. **Thiết lập Transition** (các mũi tên chuyển đổi giữa các state):
```
- Chọn mũi tên transition
- Trong Inspector:
  - Has Exit Time: không tích
  - Transition Duration: 0.1
  - Fixed Duration: tích
  - Conditions: thiết lập điều kiện chuyển state
```

5. **Điều kiện chuyển state thường dùng**:
```
Idle/Standing -> Walking:
- Speed Greater 0.01

Walking -> Idle/Standing:
- Speed Less 0.01

Idle/Walking -> Jumping:
- IsJumping = true

Jumping -> Idle/Walking:
- IsGrounded = true
```

6. **Kiểm tra trong script Movement.cs**:
```csharp
// Đảm bảo các parameter được set đúng
animator.SetFloat("Speed", Mathf.Abs(moveInput));
animator.SetBool("IsJumping", isJumping);
animator.SetBool("IsGrounded", isGrounded);
animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
```

Lưu ý quan trọng:
- Đảm bảo tên parameter trong Animator giống hệt trong code
- Loop Time và Loop Pose quan trọng cho animation lặp lại
- Transition Duration ngắn (0.1) để chuyển đổi animation mượt mà
- Không tích Has Exit Time để animation phản ứng ngay khi có input

Các animation khác (Jump, Death, Attack...) làm tương tự, chỉ khác về điều kiện transition và có thể không cần loop (ví dụ như Death animation).
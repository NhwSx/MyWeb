// TechZone Modern E-Commerce Client Script
document.addEventListener('DOMContentLoaded', function () {
    // 1. Initialize Cart Badge
    updateCartBadge();

    // 2. Initialize Wishlist Badge
    updateWishlistBadge();

    // 3. Check Top Promo Banner Dismiss State
    if (sessionStorage.getItem('techzone_top_promo_dismissed') === 'true') {
        const promo = document.getElementById('topPromoBanner');
        if (promo) promo.style.display = 'none';
    }

    // 4. Check for TempData flash messages from server and render as toasts
    const successMsg = document.getElementById('server-success-msg')?.value;
    const errorMsg = document.getElementById('server-error-msg')?.value;

    if (successMsg) {
        showToast(successMsg, 'success');
    }
    if (errorMsg) {
        showToast(errorMsg, 'error');
    }
});

// Dismiss Top Promo Banner
function dismissTopPromo() {
    const promo = document.getElementById('topPromoBanner');
    if (promo) {
        promo.style.transition = 'all 0.25s ease';
        promo.style.opacity = '0';
        promo.style.height = '0';
        promo.style.padding = '0';
        promo.style.overflow = 'hidden';
        setTimeout(() => promo.remove(), 260);
        sessionStorage.setItem('techzone_top_promo_dismissed', 'true');
    }
}

// Toast notification helper
function showToast(message, type = 'info') {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `app-toast toast-${type}`;

    let iconClass = 'bi-info-circle-fill text-warning';
    if (type === 'success') iconClass = 'bi-check-circle-fill text-success';
    if (type === 'error') iconClass = 'bi-exclamation-triangle-fill text-danger';

    toast.innerHTML = `
        <i class="bi ${iconClass} fs-5"></i>
        <div class="flex-grow-1" style="font-size: 0.875rem; font-weight: 500;">${message}</div>
        <button type="button" class="btn-close btn-close-sm" style="font-size: 0.75rem;" onclick="this.parentElement.remove()"></button>
    `;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// Fetch Cart Count for Badge
async function updateCartBadge() {
    try {
        const response = await fetch('/Cart/GetCartCount');
        if (response.ok) {
            const data = await response.json();
            const badge = document.getElementById('header-cart-badge');
            if (badge) {
                badge.innerText = data.count;
                if (data.count > 0) {
                    badge.classList.remove('d-none');
                } else {
                    badge.classList.add('d-none');
                }
            }
        }
    } catch (e) {
        console.error('Error fetching cart count:', e);
    }
}

// Wishlist Storage Management
function getWishlist() {
    try {
        return JSON.parse(localStorage.getItem('techzone_wishlist') || '[]');
    } catch {
        return [];
    }
}

function updateWishlistBadge() {
    const list = getWishlist();
    const badge = document.getElementById('header-wishlist-badge');
    if (badge) {
        badge.innerText = list.length;
    }
}

function toggleWishlist(productId, btn = null) {
    let list = getWishlist();
    const index = list.indexOf(productId);
    if (index > -1) {
        list.splice(index, 1);
        showToast('Đã xóa sản phẩm khỏi danh sách yêu thích', 'info');
        if (btn) btn.innerHTML = '<i class="bi bi-heart"></i>';
    } else {
        list.push(productId);
        showToast('Đã lưu sản phẩm vào danh sách yêu thích!', 'success');
        if (btn) btn.innerHTML = '<i class="bi bi-heart-fill text-danger"></i>';
    }
    localStorage.setItem('techzone_wishlist', JSON.stringify(list));
    updateWishlistBadge();
}

function showWishlistModal() {
    const list = getWishlist();
    const modalBody = document.getElementById('wishlistModalBody');
    if (modalBody) {
        if (list.length === 0) {
            modalBody.innerHTML = `
                <div class="py-4">
                    <i class="bi bi-heart text-muted" style="font-size: 3rem;"></i>
                    <h6 class="fw-bold mt-3 mb-1">Chưa có sản phẩm yêu thích</h6>
                    <p class="text-muted small">Hãy nhấn biểu tượng trái tim ở các sản phẩm để lưu lại xem sau.</p>
                </div>
            `;
        } else {
            modalBody.innerHTML = `
                <div class="text-start">
                    <div class="alert alert-success py-2 small mb-3">
                        <i class="bi bi-check-circle-fill me-1"></i> Bạn đang lưu <strong>${list.length}</strong> sản phẩm trong danh sách yêu thích.
                    </div>
                    <p class="text-muted small">Nhấn vào "Xem thêm sản phẩm" để khám phá thêm nhiều ưu đãi công nghệ hấp dẫn!</p>
                </div>
            `;
        }
    }
    const modalElem = document.getElementById('wishlistModal');
    if (modalElem && window.bootstrap) {
        const modal = new bootstrap.Modal(modalElem);
        modal.show();
    }
}

// AJAX Add To Cart
async function addToCart(productId, quantity = 1, btn = null) {
    let originalHtml = '';
    if (btn) {
        originalHtml = btn.innerHTML;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status"></span> Đang thêm...';
        btn.disabled = true;
    }

    try {
        const response = await fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({
                productId: productId,
                quantity: parseInt(quantity) || 1
            })
        });

        const data = await response.json();

        if (data.success) {
            showToast(data.message, 'success');
            const badge = document.getElementById('header-cart-badge');
            if (badge) {
                badge.innerText = data.cartCount;
                badge.classList.remove('d-none');
            }
        } else {
            showToast(data.message, 'error');
        }
    } catch (err) {
        showToast('Có lỗi xảy ra khi thêm vào giỏ hàng. Vui lòng thử lại!', 'error');
        console.error(err);
    } finally {
        if (btn) {
            btn.innerHTML = originalHtml;
            btn.disabled = false;
        }
    }
}

// AJAX Update Cart Item Quantity
async function updateCartItemQuantity(cartItemId, newQty, rowElementId) {
    if (newQty < 1) return;

    try {
        const response = await fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({
                cartItemId: cartItemId,
                quantity: newQty
            })
        });

        const data = await response.json();
        if (data.success) {
            // Update line item total
            const lineTotalElem = document.getElementById(`line-total-${cartItemId}`);
            if (lineTotalElem) lineTotalElem.innerText = data.lineTotal;

            // Update subtotal and grand total
            const subTotalElem = document.getElementById('cart-subtotal');
            if (subTotalElem) subTotalElem.innerText = data.subTotal;

            const grandTotalElem = document.getElementById('cart-grandtotal');
            if (grandTotalElem) grandTotalElem.innerText = data.grandTotal;

            // Update badge
            const badge = document.getElementById('header-cart-badge');
            if (badge) {
                badge.innerText = data.cartCount;
                if (data.cartCount > 0) badge.classList.remove('d-none');
            }

            showToast(data.message, 'success');
        } else {
            showToast(data.message, 'error');
        }
    } catch (err) {
        showToast('Không thể cập nhật số lượng', 'error');
        console.error(err);
    }
}

// AJAX Remove Cart Item
async function removeCartItem(cartItemId, rowElementId) {
    if (!confirm('Bạn có chắc chắn muốn bỏ sản phẩm này khỏi giỏ hàng?')) {
        return;
    }

    try {
        const response = await fetch('/Cart/RemoveItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(cartItemId)
        });

        const data = await response.json();
        if (data.success) {
            const row = document.getElementById(rowElementId);
            if (row) {
                row.remove();
            }

            const subTotalElem = document.getElementById('cart-subtotal');
            if (subTotalElem) subTotalElem.innerText = data.subTotal;

            const grandTotalElem = document.getElementById('cart-grandtotal');
            if (grandTotalElem) grandTotalElem.innerText = data.grandTotal;

            const badge = document.getElementById('header-cart-badge');
            if (badge) {
                badge.innerText = data.cartCount;
                if (data.cartCount === 0) {
                    badge.classList.add('d-none');
                    location.reload();
                }
            }

            showToast(data.message, 'success');
        } else {
            showToast(data.message, 'error');
        }
    } catch (err) {
        showToast('Không thể xóa sản phẩm', 'error');
        console.error(err);
    }
}

// Product Details: Gallery Image Switcher
function changeMainImage(url, thumbnailElement) {
    const mainImg = document.getElementById('mainProductImage');
    if (mainImg) {
        mainImg.style.opacity = '0.3';
        setTimeout(() => {
            mainImg.src = url;
            mainImg.style.opacity = '1';
        }, 150);
    }

    const thumbs = document.querySelectorAll('.product-thumb-item');
    thumbs.forEach(t => t.classList.remove('border-danger', 'shadow-sm'));
    if (thumbnailElement) {
        thumbnailElement.classList.add('border-danger', 'shadow-sm');
    }
}

// Quantity Stepper Helper
function changeQtyInput(inputId, delta, maxStock = 999) {
    const input = document.getElementById(inputId);
    if (!input) return;

    let val = parseInt(input.value) || 1;
    val += delta;

    if (val < 1) val = 1;
    if (val > maxStock) {
        val = maxStock;
        showToast(`Số lượng tối đa trong kho là ${maxStock}`, 'error');
    }

    input.value = val;
    input.dispatchEvent(new Event('change'));
}

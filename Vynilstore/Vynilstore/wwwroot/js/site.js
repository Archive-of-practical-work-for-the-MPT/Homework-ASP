// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Function to update cart count in the header
function updateCartCount(count) {
    const cartCountElement = document.getElementById('cart-count');
    if (cartCountElement) {
        cartCountElement.textContent = count;
    }
}

// Add event listeners when the DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Add to cart functionality for any add-to-cart button
    const addToCartButtons = document.querySelectorAll('.add-to-cart-btn');
    
    addToCartButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            
            const vinylId = this.dataset.vinylId;
            const vinylTitle = this.dataset.vinylTitle;
            
            // Disable button and show loading
            const originalText = this.innerHTML;
            this.disabled = true;
            this.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
            
            // Send AJAX request
            fetch('/Cart/AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: `vinylId=${vinylId}&quantity=1`
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Show success icon
                    this.innerHTML = '<i class="fas fa-check"></i>';
                    
                    // Update cart count in header
                    updateCartCount(data.totalItems);
                    
                    // Show alert
                    alert(`Пластинка "${vinylTitle}" добавлена в корзину`);
                } else {
                    // Show error icon
                    this.innerHTML = '<i class="fas fa-exclamation"></i>';
                    alert(data.message || 'Произошла ошибка при добавлении в корзину');
                }
                
                // Reset button after delay
                setTimeout(() => {
                    this.innerHTML = originalText;
                    this.disabled = false;
                }, 2000);
            })
            .catch(error => {
                console.error('Error:', error);
                this.innerHTML = '<i class="fas fa-exclamation"></i>';
                alert('Произошла ошибка при добавлении в корзину');
                
                // Reset button after delay
                setTimeout(() => {
                    this.innerHTML = originalText;
                    this.disabled = false;
                }, 2000);
            });
        });
    });
});

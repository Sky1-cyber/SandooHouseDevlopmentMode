$(document).ready(function () {
    // Get anti-forgery token
    const token = $('input[name="__RequestVerificationToken"]').val();

    // State — built from server-rendered DOM
    let orderItems = [];

    function initFromDOM() {
        orderItems = [];
        $('#orderItemsList .order-row').each(function () {
            const $row = $(this);
            const price = parseFloat($row.data('price')) || 0;
            const qty = parseInt($row.find('.item-quantity').val()) || 1;
            orderItems.push({
                id: parseInt($row.data('orderitemid')) || 0,
                menuItemId: parseInt($row.data('id')),
                brandId: parseInt($row.data('brandid')) || 0,
                price: price,
                quantity: qty,
                subtotal: price * qty
            });
            $row.find('.item-subtotal').text(fmt(price * qty));
        });
        refreshSummary();
        refreshCounts();
    }

    initFromDOM();

    // Filters
    $('#brandFilter').on('change', function () {
        const brandId = $(this).val();
        // Filter category dropdown
        $('#categoryFilter option').each(function () {
            const $opt = $(this);
            const optBrand = $opt.data('brand')?.toString() || '';
            if (!$opt.val()) return; // keep "All categories"
            $opt.toggle(!brandId || optBrand === brandId);
        });
        // Reset category if its brand is now hidden
        const selCatBrand = $('#categoryFilter option:selected').data('brand')?.toString() || '';
        if (brandId && selCatBrand !== brandId) $('#categoryFilter').val('');
        applyFilters();
    });

    $('#categoryFilter').on('change', applyFilters);
    $('#menuSearch').on('input', applyFilters);

    function applyFilters() {
        const brandId = $('#brandFilter').val();
        const catId = $('#categoryFilter').val();
        const q = $('#menuSearch').val().toLowerCase().trim();
        let visible = 0;

        $('#menuItemsList tr').each(function () {
            const $tr = $(this);
            const rowBrand = $tr.data('brand')?.toString() || '';
            const rowCat = $tr.data('category')?.toString() || '';
            const rowName = $tr.data('name') || '';

            const show = (!brandId || rowBrand === brandId)
                && (!catId || rowCat === catId)
                && (!q || rowName.includes(q));

            $tr.toggle(show);
            if (show) visible++;
        });

        $('#menuCount').text(visible + ' item' + (visible === 1 ? '' : 's'));
    }

    // Add item from catalog
    $(document).on('click', '.add-to-order', function () {
        const $btn = $(this);
        const menuItemId = parseInt($btn.data('id'));
        const name = $btn.data('name');
        const brandId = parseInt($btn.data('brandid')) || 0;
        const brandName = $btn.data('brandname') || '';
        const price = parseFloat($btn.data('price')) || 0;

        const idx = orderItems.findIndex(i => i.menuItemId === menuItemId);

        if (idx >= 0) {
            // Increment quantity
            orderItems[idx].quantity++;
            orderItems[idx].subtotal = orderItems[idx].quantity * price;

            const $row = $(`#orderItemsList .order-row[data-id="${menuItemId}"]`);
            $row.find('.item-quantity').val(orderItems[idx].quantity);
            $row.find('.item-subtotal').text(fmt(orderItems[idx].subtotal));
        } else {
            // New item
            orderItems.push({
                id: 0,
                menuItemId,
                brandId,
                price,
                quantity: 1,
                subtotal: price
            });

            $('#emptyState').remove();

            const html = `
                        <div class="order-row"
                             data-id="${menuItemId}"
                             data-price="${price}"
                             data-brandid="${brandId}"
                             data-orderitemid="0">
                            <div class="order-row-info">
                                <div class="order-row-name">${escapeHtml(name)}</div>
                                <div class="order-row-brand">${fmt(price)} each</div>
                            </div>
                            <div class="qty-ctrl">
                                <button type="button" class="qty-btn qty-dec">−</button>
                                <input type="number" class="qty-input item-quantity" value="1" min="1" />
                                <button type="button" class="qty-btn qty-inc">+</button>
                            </div>
                            <div class="row-sub item-subtotal">${fmt(price)}</div>
                            <button type="button" class="btn-remove remove-item" title="Remove item">
                                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                     stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                                    <polyline points="3 6 5 6 21 6"/>
                                    <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
                                    <path d="M10 11v6M14 11v6"/>
                                    <path d="M9 6V4h6v2"/>
                                </svg>
                            </button>
                        </div>`;
            $('#orderItemsList').append(html);
        }

        refreshSummary();
        refreshCounts();
        toast(name + ' added to order', 'success');
    });

    // Quantity — increment / decrement
    $(document).on('click', '.qty-inc', function () {
        const $input = $(this).closest('.qty-ctrl').find('.item-quantity');
        $input.val(parseInt($input.val()) + 1).trigger('change');
    });

    $(document).on('click', '.qty-dec', function () {
        const $input = $(this).closest('.qty-ctrl').find('.item-quantity');
        const cur = parseInt($input.val()) || 1;
        if (cur > 1) $input.val(cur - 1).trigger('change');
    });

    $(document).on('change', '.item-quantity', function () {
        const $row = $(this).closest('.order-row');
        const menuItemId = parseInt($row.data('id'));
        const price = parseFloat($row.data('price')) || 0;
        const qty = Math.max(1, parseInt($(this).val()) || 1);
        $(this).val(qty);

        const idx = orderItems.findIndex(i => i.menuItemId === menuItemId);
        if (idx >= 0) {
            orderItems[idx].quantity = qty;
            orderItems[idx].subtotal = qty * price;
            $row.find('.item-subtotal').text(fmt(orderItems[idx].subtotal));
        }

        refreshSummary();
    });

    // Remove item
    $(document).on('click', '.remove-item', function () {
        const $row = $(this).closest('.order-row');
        const name = $row.find('.order-row-name').text();
        const menuItemId = parseInt($row.data('id'));

        Swal.fire({
            title: 'Remove item?',
            text: `"${name}" will be removed from this order.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#FF7675',
            cancelButtonColor: '#2D3561',
            confirmButtonText: 'Yes, remove',
            cancelButtonText: 'Cancel',
            borderRadius: '16px',
            customClass: { popup: 'swal-font' }
        }).then(result => {
            if (!result.isConfirmed) return;

            orderItems = orderItems.filter(i => i.menuItemId !== menuItemId);
            $row.remove();

            if (!orderItems.length) {
                $('#orderItemsList').html(`
                            <div class="empty-state" id="emptyState">
                                <div class="empty-state-icon">
                                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none"
                                         stroke="currentColor" stroke-width="1.5"
                                         stroke-linecap="round" stroke-linejoin="round">
                                        <circle cx="9" cy="21" r="1"/>
                                        <circle cx="20" cy="21" r="1"/>
                                        <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/>
                                    </svg>
                                </div>
                                <p>No items yet — add from the catalog</p>
                            </div>`);
            }

            refreshSummary();
            refreshCounts();
            toast(name + ' removed', 'error');
        });
    });

    // Summary recalculation
    $('#discountAmount, #cashReceived').on('input', refreshSummary);

    function refreshSummary() {
        const subtotal = orderItems.reduce((s, i) => s + i.subtotal, 0);
        const discount = Math.max(0, parseFloat($('#discountAmount').val()) || 0);
        const cash = parseFloat($('#cashReceived').val()) || 0;
        const total = Math.max(0, subtotal - discount);
        const change = cash - total;

        $('#subtotalDisplay').text(fmt(subtotal));
        $('#totalDisplay').text(fmt(total));

        const $chg = $('#changeDisplay');
        $chg.text(fmt(Math.max(0, change)));
        $chg.removeClass('change-positive change-negative');

        const $cash = $('#cashReceived');
        if (cash > 0 && change < 0) {
            $chg.addClass('change-negative');
            $cash.addClass('invalid');
        } else if (cash > 0 && change >= 0) {
            $chg.addClass('change-positive');
            $cash.removeClass('invalid');
        } else {
            $cash.removeClass('invalid');
        }
    }

    // Count badges
    function refreshCounts() {
        const n = orderItems.length;
        $('#orderCount').text(n);
        $('#itemCountDisplay').text(n + ' item' + (n === 1 ? '' : 's'));
    }

    // Submit
    $('#updateOrderBtn').on('click', function () {
        if (!orderItems.length) {
            Swal.fire({
                icon: 'error',
                title: 'Empty order',
                text: 'Please add at least one item before saving.',
                confirmButtonColor: '#FF6B35'
            });
            return;
        }

        const cash = parseFloat($('#cashReceived').val()) || 0;
        const discount = parseFloat($('#discountAmount').val()) || 0;
        const subtotal = orderItems.reduce((s, i) => s + i.subtotal, 0);
        const total = Math.max(0, subtotal - discount);

        if (cash > 0 && cash < total) {
            Swal.fire({
                icon: 'warning',
                title: 'Insufficient payment',
                html: `Cash received <strong>${fmt(cash)}</strong> is less than the total <strong>${fmt(total)}</strong>.<br>Continue anyway?`,
                showCancelButton: true,
                confirmButtonColor: '#FF6B35',
                cancelButtonColor: '#2D3561',
                confirmButtonText: 'Yes, save anyway',
                cancelButtonText: 'Review'
            }).then(result => {
                if (result.isConfirmed) submitOrder();
            });
        } else {
            submitOrder();
        }
    });

    function submitOrder() {
        const payload = {
            id: parseInt($('#orderId').val()),
            orderStatus: parseInt($('#orderStatus').val()),
            discountAmount: parseFloat($('#discountAmount').val()) || 0,
            cashReceived: parseFloat($('#cashReceived').val()) || 0,
            items: orderItems.map(i => ({
                id: i.id,
                menuItemId: i.menuItemId,
                brandId: i.brandId,
                quantity: i.quantity,
                price: i.price,
                subtotal: i.subtotal
            }))
        };

        const $btn = $('#updateOrderBtn');
        const orig = $btn.html();
        $btn.prop('disabled', true).html('<span class="spin">⟳</span>&nbsp; Saving…');

        $.ajax({
            url: '/Order/EditOrder',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': token
            },
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Saved!',
                        text: res.message || 'Order updated successfully.',
                        showConfirmButton: false,
                        timer: 1600,
                        confirmButtonColor: '#FF6B35'
                    }).then(() => {
                        window.location.href = '/Home/Index';
                    });
                } else {
                    $btn.prop('disabled', false).html(orig);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: res.error || 'Something went wrong.',
                        confirmButtonColor: '#FF6B35'
                    });
                }
            },
            error: function (xhr) {
                $btn.prop('disabled', false).html(orig);
                const msg = xhr.responseJSON?.error || 'An unexpected error occurred.';
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: msg,
                    confirmButtonColor: '#FF6B35'
                });
                console.error('[EditOrder] AJAX error:', xhr);
            }
        });
    }

    // Helpers
    function fmt(n) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(n);
    }

    function escapeHtml(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function toast(msg, type) {
        const icon = type === 'success' ? 'success' : 'error';
        Swal.fire({
            icon,
            text: msg,
            showConfirmButton: false,
            timer: 1400,
            toast: true,
            position: 'top-end',
            customClass: { popup: 'swal-font' }
        });
    }
});
$(document).ready(function () {
    $('#tabs').tabs();
    $('.customer-connect-button').live('click', function () {
        var $select = $('<select/>');
        $.each($('.vender'), function (index, item) {
            var option = $('<option/>');
            var $vender = $(this);
            option.val($vender.attr('data-vender-id'));
            option.text($vender.text());
            $select.append(option);
        });

        $('div.customer-connect-popup').remove();
        var $popup = $('<div style="position:absolute;background-color:white;border:1px solid black;padding:2px;" class="customer-connect-popup"/>');
        $popup.append($select);
        var $button = $(this);
        var pos = $button.position();
        pos.left += $button.outerWidth();
        $popup.css({ left: pos.left, top: pos.top });

        var submitButton = $('<button type="button">Connect</button>');
        var cancelButton = $('<button type="button">Cancel</button>');
        $popup.append(submitButton).append(cancelButton);

        submitButton.click(function () {
            $('#customerId').val($button.attr('data-customer-id'));
            $('#venueId').val($('option:selected', $select).val());
            $('#connect-button')[0].click();
            $('div.customer-connect-popup').remove();
        });

        cancelButton.click(function () {
            $('div.customer-connect-popup').remove();
        });

        $button.closest('td').append($popup);
    });

    $('button.place-order-button').live('click', function () {
        var $button = $(this);
        $.ajax({
            url: 'iPadTest/PlaceRandomOrder',
            type: 'post',
            data: {
                "venueId": $button.attr('data-venue-id') * 1,
                "customerId": $button.attr('data-customer-id') * 1
            },
            success: function (data, status) {
                $('#connection_list').html(data);
            },
            error: function () {
                alert('error. please click refresh button of refresh your browser.');
            }
        });
    });

    $('#connection-refresh-button').click(function () {
        $.each($('#connectionRefreshForm input:hidden'), function (index, item) {
            $(item).val('');
        });

        $('#connect-button')[0].click();
    });

    $('button.view-menu-link').live('click', function () {
        $('#view-menu-dialog').dialog('close').remove();
        $('#busy-wait').show();
        $.ajax({
            url: 'iPadTest/Menus/' + $(this).attr('data-venue-id'),
            success: function (data, status) {
                $dialog = $('<div id="view-menu-dialog" title="Menu/Category/Item/Option"></div>');
                $dialog.html(data);
                $dialog.dialog({ height: 600, width: 800 });
            },
            error: function () {
                alert('error. please click refresh button of refresh your browser.');
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });

        return false;
    });

    $('button.view-ipad-link').live('click', function () {
        $('#view-ipad-dialog').dialog('close').remove();
        $('#busy-wait').show();
        $.ajax({
            url: 'iPadTest/iPads/' + $(this).attr('data-venue-id'),
            success: function (data, status) {
                $dialog = $('<div id="view-ipad-dialog" title="iPads"></div>');
                $dialog.html(data);
                $dialog.dialog({ height: 600, width: 800 });
            },
            error: function () {
                alert('error. please click refresh button of refresh your browser.');
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });

        return false;
    });

    $('button.view-order-button').live('click', function () {
        $('#view-order-detail-dialog').dialog('close').remove();
        $('#busy-wait').show();
        var $button = $(this);
        var $controller = $button.attr('data-controller');
        $.ajax({
            url: $controller + '/VenueCustomerOrders/' + $button.attr('data-venue-id') + '?customerId=' + $button.attr('data-customer-id'),
            success: function (data, status) {
                $dialog = $('<div id="view-order-detail-dialog" title="Order Detail"></div>');
                $dialog.html(data);
                $dialog.dialog({ height: 600, width: 800 });
            },
            error: function () {
                alert('error. please click refresh button of refresh your browser.');
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });

        return false;
    });

    $('button.message-to-customer-button').live('click', function () {
        var message = prompt("Enter message");
        if ($.trim(message) != '') {
            var $button = $(this);
            var $controller = $button.attr('data-controller');
            $.ajax({
                url: $controller + '/SendMessageToCustomer',
                type: 'post',
                data: {
                    venueId: $button.attr('data-venue-id'),
                    customerId: $button.attr('data-customer-id'),
                    message: message
                }
            });
        }
    });

    $('button.accept-connection-button').live('click', function () {
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            url: 'iPhoneTest/AcceptConnection/' + $(this).attr('data-venue-id') + '?customerId=' + $(this).attr('data-customer-id'),
            success: function (data, status) {
                $('#connection_list').html(data);
            },
            error: function () {
                alert('error. please click refresh button of refresh your browser.');
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.reject-connection-button').live('click', function () {
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            url: 'iPhoneTest/RejectConnection/' + $(this).attr('data-venue-id') + '?customerId=' + $(this).attr('data-customer-id'),
            success: function (data, status) {
                $('#connection_list').html(data);
            },
            error: function () {
                alert('error. please click refresh button of refresh your browser.');
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.confirm-order').live('click', function () {
        var $button = $(this);
        var orderItemId = $(this).attr('data-order-item-id');
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: 'iPhoneTest/ConfirmOrderItem/' + orderItemId,
            success: function (data, status) {
                if (data.error == null) {
                    $button.parent()
                        .append($('<button type="button" data-order-item-id="' + orderItemId + ' "class="order-processed">Process</button>'))
                        .append($('<small/>').text("(Assigned to " + data.ipad + ")"));
                    $button.remove();
                }
                else {
                    alert('unexpected result: ' + data.error);
                }
            },
            error: function () {
                alert('error. please click refresh button of refresh your browser.');
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.order-processed').live('click', function () {
        var $button = $(this);
        var orderItemId = $(this).attr('data-order-item-id');
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            dataType: 'json',
            url: 'iPhoneTest/ProcessOrderItem/' + orderItemId,
            success: function (data, status) {
                var parent = $button.parent();
                var $dialog = parent.closest('.ui-dialog');
                parent.empty();
                parent.append($('<span>Processed </span>'))
                      .append($('<small/>').text("(Assigned to \"" + data.ipad + "\")"));
                if ($('button.confirm-order, button.order-processed', $dialog).length == 0) {
                    $('button.finalize-order', $dialog).show();
                }
            },
            error: function () {
                alert('error. please click refresh button of refresh your browser.');
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.finalize-order').live('click', function () {
        var $button = $(this);
        var orderItemId = $(this).attr('data-order-item-id');
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            url: 'iPhoneTest/FinaizeCustomerOrder',
            data: {
                "venueId": $button.attr('data-venue-id') * 1,
                "customerId": $button.attr('data-customer-id') * 1
            },
            success: function (data, status) {
                var dialog = $button.closest('#view-order-detail-dialog');
                dialog.dialog('close');
                $('#connection_list').html(data);
            },
            error: function (jqXHR, textStatus, errorThrow) {
                alert('(' + textStatus + ')\n' + jqXHR.responseText);
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.simulate-close-button').live('click', function () {
        var $button = $(this);
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            url: 'iPadTest/SimulateCloseRequest',
            data: {
                "venueId": $button.attr('data-venue-id') * 1,
                "customerId": $button.attr('data-customer-id') * 1
            },
            success: function (data, status) {
                var dialog = $button.closest('#view-order-detail-dialog');
                dialog.dialog('close');
                $('#connection_list').html(data);
            },
            error: function (jqXHR, textStatus, errorThrow) {
                alert('(' + textStatus + ')\n' + jqXHR.responseText);
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.cancel-order-button').live('click', function () {
        var $button = $(this);
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            url: 'iPhoneTest/CancelOrder',
            data: {
                "venueId": $button.attr('data-venue-id') * 1,
                "orderId": $button.attr('data-order-id') * 1
            },
            success: function (data, status) {
                var dialog = $button.closest('#view-order-detail-dialog');
                dialog.dialog('close');
            },
            error: function (jqXHR, textStatus, errorThrow) {
                alert('(' + textStatus + ')\n' + jqXHR.responseText);
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.undo-order-item-button').live('click', function () {
        var $button = $(this);
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            url: 'iPhoneTest/UndoOrderItem',
            data: {
                "venueId": $button.attr('data-venue-id') * 1,
                "iPadId": $button.attr('data-ipad-id') * 1,
                "orderItemId": $button.attr('data-orderitem-id') * 1
            },
            success: function (data, status) {
                var dialog = $button.closest('#view-order-detail-dialog');
                dialog.dialog('close');
                alert('Please reopen the dialog');
            },
            error: function (jqXHR, textStatus, errorThrow) {
                alert('(' + textStatus + ')\n' + jqXHR.responseText);
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.cancel-order-item-button').live('click', function () {
        var $button = $(this);
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            url: 'iPhoneTest/CancelOrderItem',
            data: {
                "venueId": $button.attr('data-venue-id') * 1,
                "iPadId": $button.attr('data-ipad-id') * 1,
                "orderItemId": $button.attr('data-orderitem-id') * 1
            },
            success: function (data, status) {
                var dialog = $button.closest('#view-order-detail-dialog');
                dialog.dialog('close');
                alert('Please reopen the dialog');
            },
            error: function (jqXHR, textStatus, errorThrow) {
                alert('(' + textStatus + ')\n' + jqXHR.responseText);
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });

    $('button.pickup-button').live('click', function() {
        var $button = $(this);
        $('#busy-wait').show();
        $.ajax({
            type: 'post',
            url: 'iPadTest/ConfirmPickup',
            data: {
                "customerId": $button.attr('data-customer-id') * 1, 
                "orderItemId": $button.attr('data-orderitem-id') * 1
            },
            success: function (data, status) {
                var dialog = $button.closest('#view-order-detail-dialog');
                dialog.dialog('close');
                alert('Please reopen the dialog');
            },
            error: function (jqXHR, textStatus, errorThrow) {
                alert('(' + textStatus + ')\n' + jqXHR.responseText);
            },
            complete: function () {
                $('#busy-wait').hide();
            }
        });
    });
});
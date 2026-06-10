$(function () {
    const dt = $('#usersTable').DataTable({
        processing: true,
        serverSide: true,
        ajax: function (dtReq, callback) {
            const pageIndex = Math.trunc(dtReq.start / dtReq.length);
            const pageSize = dtReq.length;
            const search = dtReq.search?.value || '';

            const orderIdx = dtReq.order?.[0]?.column ?? 0;
            const sortCol = dtReq.columns?.[orderIdx]?.data || 'id';
            const sortDir = dtReq.order?.[0]?.dir || 'asc';

            console.log("pageIndex", pageIndex);
            console.log("pageSize", pageSize);

            const withDeleted = $('#chkShowDeleted').is(':checked');

            $.ajax({
                url: '/Users/GetList?withDeleted=' + withDeleted,
                type: 'POST',
                data: {
                    Index: pageIndex,
                    Size: pageSize,
                    //Search: search,
                    //SortColumn: sortCol,
                    //SortDirection: sortDir
                },

                success: function (res) {
                    console.log("res", res);

                    const items = res.items ?? [];

                    callback({
                        draw: dtReq.draw,
                        recordsTotal: res.noOfItem ?? 0,
                        recordsFiltered: res.noOfItem ?? 0,
                        data: items
                    });
                },
                error: function () {
                    callback({ draw: dtReq.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                }
            });
        },
        columns: [
            { data: 'userId', title: 'Id' },
            { data: 'username', title: 'Kullanıcı Adı' },
            { data: 'fullName', title: 'Tam Ad' },

            // Şifre (görsel uyarı: normalde hash gösterilir veya hiç gösterilmez)
            //{ data: 'passwordHash', title: 'Şifre', visible: false, },

            {
                data: 'userRoles',
                title: 'Roller',
                render: function (claims) {
                    if (!claims || !Array.isArray(claims)) return '';

                    // name alanlarını alıp virgül ile birleştiriyoruz
                    return claims.map(c => c.role.name).join(', ');
                }
            },
            {
                data: 'isEnabled',
                title: 'Durum',
                render: function (d) {
                    return d === true || d === 1
                        ? '<span class="badge bg-success">Aktif</span>'
                        : '<span class="badge bg-danger">Pasif</span>';
                }
            },
            {
                data: 'requiresLdapAuthentication',
                title: 'Giriş Tipi',
                render: function (d) {
                    return d === true || d === 1
                        ? '<span class="badge bg-label-primary">LDAP</span>'
                        : '<span class="badge bg-label-secondary">Lokal</span>';
                }
            },
            {
                data: null,
                title: 'İşlem',
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    return `
                        <div class="dt-actions">
                            <a href="#" class="btn-action js-edit" data-id="${row.userId}" title="Düzenle">
                            <i class="bx bx-edit"></i>
                        </a>
                        <a href="#" class="btn-action delete js-delete" data-id="${row.userId}" title="Sil">
                            <i class="bx bx-trash"></i>
                        </a>
                    </div>`;
                }
            }
        ],

        order: [[0, 'desc']],

        dom:
            "<'row mx-2'<'col-md-2'l>" +
            "<'col-md-10 dt-action-buttons d-flex align-items-center justify-content-md-end justify-content-center flex-wrap gap-2 mb-3 mb-md-0'fB>>" +
            "<'row'<'col-12'tr>>" +
            "<'row mx-2 mt-2'<'col-sm-12 col-md-6'i><'col-sm-12 col-md-6'p>>",

        buttons: [
            {
                extend: 'collection',
                text: '<i class="bx bx-upload me-1"></i>Dışa Aktar',
                className: 'btn btn-secondary btn-label-secondary mx-3',
                autoClose: true,
                buttons: [
                    { extend: 'copyHtml5', exportOptions: { columns: [0, 1, 2, 3, 4, 5] } },
                    { extend: 'excelHtml5', title: 'Kullanıcılar', exportOptions: { columns: [0, 1, 2, 3, 4, 5] } },
                    { extend: 'csvHtml5', title: 'Kullanıcılar', exportOptions: { columns: [0, 1, 2, 3, 4, 5] } },
                    { extend: 'pdfHtml5', title: 'Kullanıcılar', exportOptions: { columns: [0, 1, 2, 3, 4, 5] }, orientation: 'landscape' },
                    { extend: 'print', title: 'Kullanıcılar', exportOptions: { columns: [0, 1, 2, 3, 4, 5] } }
                ]
            },
            {
                text: '<i class="bx bx-plus me-0 me-lg-2"></i><span class="d-none d-lg-inline-block">Kullanıcı Ekle</span>',
                className: 'btn btn-primary',
                attr: {
                    id: 'btnAddUser',
                    'data-bs-toggle': 'offcanvas',
                    'data-bs-target': '#offcanvasAddUser'
                }
            }
        ],

        lengthMenu: [10, 25, 50, 100],
        language: {
            //processing: 'Yükleniyor...',
            //search: 'Ara...',
            url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json"
        }
    });

    dt.on('init', function () {
        const $filter = $('.dataTables_filter');
        $filter.find('label').contents().filter(function () { return this.nodeType === 3; }).remove(); // "Search" metnini sil
        $filter.find('input')
            .attr('placeholder', 'Ara…')      // placeholder
            .addClass('form-control me-2');    // bootstrap uyumu + sağ boşluk
    });

    dt.on('init', function () {
        const $filter = $('#usersTable_filter'); // DataTables filter container

        // Daha önce eklenmişse tekrar ekleme
        if ($filter.find('#chkShowDeleted').length === 0) {

            const $wrap = $(`
            <div class="d-flex align-items-center me-2">
                <div class="form-check mb-0">
                    <input class="form-check-input" type="checkbox" id="chkShowDeleted">
                    <label class="form-check-label" for="chkShowDeleted">Silinenleri Getir</label>
                </div>
            </div>
            `);

            $wrap.insertBefore($filter);
        }
    });

    $(document).on('change', '#chkShowDeleted', function () {
        dt.ajax.reload(null, true);
    });

    $('#operationClaimId').select2({
        placeholder: 'Rol seçiniz',
        width: '100%',
        dropdownParent: $('#offcanvasAddUser'),
        closeOnSelect: false
    });

    $('#edit-operationClaimId').select2({
        placeholder: 'Rol seçiniz',
        width: '100%',
        dropdownParent: $('#offcanvasEditUser'),
        closeOnSelect: false
    });

    function syncPasswordInput($ldapCheckbox, $passwordInput) {
        const requiresLdap = $ldapCheckbox.is(':checked');
        $passwordInput.prop('disabled', requiresLdap);
        if (requiresLdap) {
            $passwordInput.val('');
        }
    }

    $('#requiresLdapAuthentication').on('change', function () {
        syncPasswordInput($(this), $('#password'));
    });

    $('#edit-requiresLdapAuthentication').on('change', function () {
        syncPasswordInput($(this), $('#edit-password'));
    });

    syncPasswordInput($('#requiresLdapAuthentication'), $('#password'));
    syncPasswordInput($('#edit-requiresLdapAuthentication'), $('#edit-password'));

    $(document).on('click', '#btnAddUser', function () {
        $('#addNewUserForm')[0].reset();
        $('#requiresLdapAuthentication').prop('checked', true);
        $('#operationClaimId').val([]).trigger('change');
        syncPasswordInput($('#requiresLdapAuthentication'), $('#password'));
    });

    $.ajax({
        url: '/Roles/GetList',
        type: 'GET',
        dataType: 'json',
        success: function (res) {
            const items = res ?? [];

            // Selectleri temizle
            $('#operationClaimId').empty();
            $('#edit-operationClaimId').empty();

            //Roller ekleniyor
            items.forEach(x => {
                if (x && x.roleId !== undefined && x.name) {
                    $('#operationClaimId').append(`<option value="${x.roleId}">${x.name}</option>`);
                    $('#edit-operationClaimId').append(`<option value="${x.roleId}">${x.name}</option>`);
                }
            });

            // Select2 refresh (gerekli!)
            $('#operationClaimId').trigger('change');
            $('#edit-operationClaimId').trigger('change');
        },
        error: function (xhr) {
            console.error("Rol listesi alınamadı", xhr);
        }
    });

    $('#addNewUserForm').on('submit', function (e) {
        e.preventDefault();

        // Select2 multiple → array döner (["1","2"])
        const selectedRoleIds = ($('#operationClaimId').val() || []).filter(Boolean);

        // API'ye göndereceğimiz UserRoles koleksiyonu
        const userRoles = selectedRoleIds.map(roleId => ({
            roleId: parseInt(roleId, 10)
            // userId eklemeye gerek YOK → backend kendi UserId atadıktan sonra ilişkilendirir
        }));



        const createUserCommand = {
            username: $('#firstName').val()?.trim(),
            //lastName: $('#lastName').val()?.trim(),
            fullName: $('#fullName').val()?.trim(),
            passwordHash: $('#password').val(),
            isEnabled: $('#status').is(':checked'),
            requiresLdapAuthentication: $('#requiresLdapAuthentication').is(':checked'),
            userRoles: userRoles
        };

        $.ajax({
            url: '/Users/Create',
            type: 'POST',
            data: createUserCommand,
            success: function (res) {
                const offcanvasEl = document.getElementById('offcanvasAddUser');
                const offc = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
                offc.hide();

                $('#addNewUserForm')[0].reset();
                $('#operationClaimId').val([]).trigger('change');
                syncPasswordInput($('#requiresLdapAuthentication'), $('#password'));

                dt.ajax.reload(null, false);

                Toast.fire({ icon: 'success', title: 'Kullanıcı başarıyla kaydedildi.' });
            },
            error: function (xhr) {
                parseErrorResponse(xhr);
            }
        });
    });

    function getRowDataFromBtn(btn) {
        const $tr = $(btn).closest('tr');
        const tr = $tr.hasClass('child') ? $tr.prev() : $tr;
        return dt.row(tr).data();
    }

    $('#usersTable').on('click', '.js-edit', function (e) {
        e.preventDefault();
        const u = getRowDataFromBtn(this);
        if (!u) return;

        $('#edit-id').val(u.userId);
        $('#edit-firstName').val(u.username || '');
        //$('#edit-lastName').val(u.lastName || '');
        $('#edit-fullName').val(u.fullName || '');
        $('#edit-password').val('');
        $('#edit-status').prop('checked', !!u.isEnabled);
        $('#edit-requiresLdapAuthentication').prop('checked', u.requiresLdapAuthentication !== false);
        syncPasswordInput($('#edit-requiresLdapAuthentication'), $('#edit-password'));

        const roleIds = (u.userRoles || [])
            .map(ur => ur.roleId)
            .filter(id => id != null)
            .map(String);


        $('#edit-operationClaimId').val(roleIds).trigger('change');

        bootstrap.Offcanvas.getOrCreateInstance('#offcanvasEditUser').show();
    });

    $('#editUserForm').on('submit', function (e) {
        e.preventDefault();

        const userId = parseInt($('#edit-id').val(), 10);

        // Select2 multiple → array döner (["1","2"])
        const selectedRoleIds = ($('#edit-operationClaimId').val() || []).filter(Boolean);

        // API'ye göndereceğimiz UserRoles koleksiyonu
        const userRoles = selectedRoleIds.map(function (rid) {
            return {
                userId: userId,
                roleId: parseInt(rid, 10)
            };
        });

        const updateUserCommand = {
            userId: userId,
            userName: $('#edit-firstName').val()?.trim(),
            fullName: $('#edit-fullName').val()?.trim(),
            passwordHash: $('#edit-password').val() || null,
            isEnabled: $('#edit-status').is(':checked'),
            requiresLdapAuthentication: $('#edit-requiresLdapAuthentication').is(':checked'),
            userRoles: userRoles          // 🔥 artık koleksiyon gidiyor
        };

        $.ajax({
            url: '/Users/Update',
            type: 'POST',
            data: updateUserCommand,

            success: function () {
                bootstrap.Offcanvas.getOrCreateInstance('#offcanvasEditUser').hide();
                $('#editUserForm')[0].reset();
                dt.ajax.reload(null, false);
                Toast.fire({ icon: 'success', title: 'Kullanıcı başarıyla güncellendi.' });
            },
            error: function (xhr) {
                parseErrorResponse(xhr);
            }
        });
    });

    $('#usersTable').on('click', '.js-delete', function (e) {
        e.preventDefault();

        const id = $(this).data('id');

        Swal.fire({
            title: 'Kullanıcıyı silmek istediğinizden emin miniz?',
            text: 'Kullanıcı pasifleştirildikten sonra sisteme giriş yapamayacaktır.',
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sil!",
            cancelButtonText: "Vazgeç",
            confirmButtonColor: "#d33",
            cancelButtonColor: "#6c757d"
        }).then((result) => {
            if (!result.isConfirmed) return;

            $.ajax({
                url: `/Users/Delete/`,
                type: 'POST',
                data: { id: id },

                success: function () {
                    dt.ajax.reload(null, false);
                    Toast.fire({ icon: 'success', title: 'Kullanıcı başarıyla pasifleştirildi.' });
                },
                error: function (xhr) {
                    parseErrorResponse(xhr);
                }
            });
        });
    });
});

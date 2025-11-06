document.addEventListener("DOMContentLoaded", () => {
    const antiForgeryInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const antiforgeryToken = antiForgeryInput ? antiForgeryInput.value : null;

    document.querySelectorAll('.assign-arrow').forEach(btn => {
        btn.addEventListener('click', async () => {
            const bankCat = btn.dataset.bankcat;
            const trxtype = btn.dataset.trxtype;

            try {
                const payload = {
                    bankCategoryName: bankCat,
                    transactionType: trxtype
                };
                const headers = { 'Content-Type': 'application/json' };
                if (antiforgeryToken) headers['RequestVerificationToken'] = antiforgeryToken;
                const res = await fetch("/Categories/CreateFromBankCategory/", {
                    method: 'POST',
                    headers,
                    body: JSON.stringify(payload)
                });
                if (!res.ok) throw new Error('Server returned ' + res.status);
                const data = await res.json(); // { id, name, existed }

                // Update ALL selects (rows) with same parsed category + trx type
                document.querySelectorAll('.category-select').forEach(select => {
                    if (select.dataset.trxtype === trxtype) {
                        let option = Array.from(select.options).find(o => o.value == data.id);
                        if (!option) {
                            option = new Option(data.name, data.id);
                            option.dataset.existed = data.existed;
                            select.add(option);
                        } else {
                            option.text = data.name;
                            option.dataset.existed = data.existed;
                        }

                        if (select.dataset.parsed === bankCat) {
                            select.value = data.id;
                            updateCategoryClass(select);
                        }
                    }
                });
            } catch (err) {
                console.error(err);
                alert('Failed to create category: ' + (err.message || err));
            }
        });
    });


    document.querySelectorAll('.category-select').forEach(select => {
        select.addEventListener('change', () => {
            updateCategoryClass(select);
            select.blur();  
        });
        updateCategoryClass(select);
    });

    function updateCategoryClass(select) {
        select.classList.remove("category-existing", "category-new", "category-uncategorized");

        const selectedOption = select.options[select.selectedIndex];
        const existed = selectedOption?.dataset.existed === "true";
        console.log(selectedOption.dataset.existed);

        if (select.value && select.value !== "1" && existed) {
            select.classList.add("category-existing");
        } else if (select.value && select.value !== "1") {
            select.classList.add("category-new");
        } else {
            select.classList.add("category-uncategorized");
        }

        const parsedName = select.dataset.parsed;
        const selectedText = select.options[select.selectedIndex]?.text?.trim();
        if (selectedText && selectedText === parsedName) {
            const btn = document.querySelector(`.assign-arrow[data-row="${select.dataset.row}"]`);
            if (btn) btn.disabled = true;
        }
    }
});

//     function toggleNode(el) {
//     const children = el.parentElement.querySelector(".tree-children");
//     if (!children) return;
//
//     children.classList.toggle("collapsed");
//     el.textContent = children.classList.contains("collapsed") ? "▶" : "▼";
// }
//


document.addEventListener("DOMContentLoaded", function () {
    // Handle toggle clicks
    document.querySelectorAll(".tree-toggle").forEach(function (toggle) {
        toggle.addEventListener("click", function (e) {
            // prevent click on links inside .tree-actions from toggling
            if (e.target.closest(".tree-actions")) {
                return;
            }

            const sublist = toggle.parentElement.querySelector(".subcategory-list");
            const icon = toggle.querySelector(".toggle-icon");

            if (sublist) {
                sublist.classList.toggle("open");
                if (icon) {
                    icon.classList.toggle("open");
                }
            }
        });
    });
});


document.addEventListener("DOMContentLoaded", function () {
    const parentSelect = document.getElementById("ParentCategory");
    const typeSelect = document.getElementById("CategoryType");

    parentSelect.addEventListener("change", function () {
        const selected = parentSelect.options[parentSelect.selectedIndex];
        const parentType = selected.getAttribute("data-type");
        console.log(parentType);
        console.log(parentSelect.value);
        if (parentType != null){
            typeSelect.value = parentType;
            typeSelect.disabled = true;
        }
        else{
            typeSelect.disabled = false;
            typeSelect.value = "";
        }
    });
});
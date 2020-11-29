select * from SO_Header where Last_Updated > '2020-11-24'
select * from SO_Detail where Last_Updated > '2020-11-24' and Material = '503194747'

select * from SO_Detail where Sales_Order = '68624'
select * from SO_Detail where Sales_Order = '69516'

select a.Ship_To_ID from SO_Header h join Address a on a.Address = h.Ship_To where Sales_Order = '69453'
select * from Address where Customer = 'BRPFIN.CAD'

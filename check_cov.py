import xml.etree.ElementTree as ET
import sys
path = sys.argv[1]
patterns = sys.argv[2:]
tree = ET.parse(path)
for cls in tree.getroot().iter('class'):
    name = cls.get('name','')
    if any(p in name for p in patterns):
        lines = cls.find('lines')
        if lines is None: continue
        total=0; cov=0
        for l in lines.findall('line'):
            total+=1
            if int(l.get('hits','0'))>0: cov+=1
        if total==0: continue
        pct = cov*100.0/total
        fn = cls.get('filename')
        print(f"{cov}/{total} ({pct:.1f}%) - {name} ({fn})")
